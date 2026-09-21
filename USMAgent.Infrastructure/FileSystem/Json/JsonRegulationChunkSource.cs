using Microsoft.Extensions.Options;
using System.Text.Json;
using USMAgent.Application.Abstractions;
using USMAgent.Application.Models;

namespace USMAgent.Infrastructure.FileSystem.Json;

public sealed class JsonRegulationChunkSource : IRegulationChunkSource
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly Sha256FileHashService _fileHash;
    private readonly IndexingOptions _options;

    public JsonRegulationChunkSource(Sha256FileHashService fileHash, IOptions<IndexingOptions> options)
    {
        _fileHash = fileHash;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<RegulationChunkDocument>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var root = ResolvePath(_options.ChunksPath);

        var documents = new List<RegulationChunkDocument>();

        foreach (var filePath in Directory.EnumerateFiles(root, "*.json", SearchOption.AllDirectories))
        {
            var fileInfo = new FileInfo(filePath);

            documents.Add(new RegulationChunkDocument
            {
                Id = filePath,
                Name = Path.GetFileName(filePath),
                Hash = await _fileHash.ComputeSha256Async(filePath, cancellationToken),
                Size = fileInfo.Length,
                LastModifiedUtc = fileInfo.LastWriteTimeUtc
            });
        }

        return documents;
    }

    public async Task<IReadOnlyList<RegulationChunk>> ReadAsync(
        RegulationChunkDocument document,
        CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(document.Id);

        var chunks = await JsonSerializer.DeserializeAsync<List<RegulationChunk>>(
            stream,
            JsonOptions,
            cancellationToken);

        return chunks ?? [];
    }

    private static string ResolvePath(string path) => Path.IsPathRooted(path)
        ? path
        : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, path));
}
