using Microsoft.Extensions.Options;
using System.Text.Json;
using USMAgent.Application.Abstractions;
using USMAgent.Application.Models;
using USMAgent.Application.Options;

namespace USMAgent.Application;

public sealed class RegulationIndexer
{
    private const string VectorSizeProbeText = "Text to store embending number";

    private readonly ITextEmbeddingGenerator _embeddings;
    private readonly IRegulationIndexStore _store;
    private readonly IProcessedFilesStore _processedFiles;
    private readonly IFileHashService _fileHash;
    private readonly IndexingOptions _options;

    public RegulationIndexer(
        ITextEmbeddingGenerator embeddings,
        IRegulationIndexStore store,
        IProcessedFilesStore processedFiles,
        IFileHashService fileHash,
        IOptions<IndexingOptions> options)
    {
        _embeddings = embeddings;
        _store = store;
        _processedFiles = processedFiles;
        _fileHash = fileHash;
        _options = options.Value;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var processed = await _processedFiles.LoadAsync(cancellationToken);

        await EnsureCollectionAsync(cancellationToken);
        
        var chunksRoot = ResolvePath(_options.ChunksPath);

        foreach (var filePath in Directory.EnumerateFiles(chunksRoot, "*.json", SearchOption.AllDirectories))
        {
            var sha256 = await _fileHash.ComputeSha256Async(filePath, cancellationToken);

            if (processed.Any(x => string.Equals(x.Sha256, sha256, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            await UploadFileAsync(filePath, cancellationToken);

            processed.Add(CreateRecord(filePath, sha256));

            await _processedFiles.SaveAsync(processed, cancellationToken);
        }
    }

    private async Task EnsureCollectionAsync(CancellationToken cancellationToken)
    {
        var probe = await _embeddings.GenerateAsync(VectorSizeProbeText, cancellationToken)
            ?? throw new InvalidOperationException("Failed to determine embedding vector size.");
        
        await _store.EnsureCollectionAsync((uint)probe.Length, cancellationToken);
    }

    private async Task<int> UploadFileAsync(string filePath, CancellationToken cancellationToken)
    {
        var json = await File.ReadAllTextAsync(filePath, cancellationToken);

        var chunks = JsonSerializer.Deserialize<List<RegulationChunk>>(json) ?? [];

        var items = new List<(RegulationChunk, float[])>(chunks.Count);

        foreach (var chunk in chunks)
        {
            var vector = await _embeddings.GenerateAsync(chunk.EmbeddingText, cancellationToken);

            if (vector is null || vector.Length == 0)
                throw new InvalidOperationException($"Failed to embed chunk {chunk.ChunkIndex} of {filePath}.");

            items.Add((chunk, vector));
        }

        await _store.UpsertAsync(items, cancellationToken);

        return items.Count;
    }

    private ProcessedFileRecord CreateRecord(string filePath, string sha256)
    {
        var fileInfo = new FileInfo(filePath);

        return new ProcessedFileRecord
        {
            FileName = Path.GetFileName(filePath),
            RelativePath = $"QdrantStorage/{_options.ChunksPath}",
            Sha256 = sha256,
            Size = fileInfo.Length,
            LastWriteTimeUtc = fileInfo.LastWriteTimeUtc,
            ProcessedAtUtc = DateTime.UtcNow,
            ProcesedFilename = "qdrantstorage"
        };
    }

    private static string ResolvePath(string path) => Path.IsPathRooted(path)
        ? path
        : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, path));
}