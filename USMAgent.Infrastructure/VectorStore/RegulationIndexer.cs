using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using USMAgent.Application.Abstractions;
using USMAgent.Application.Models;
using USMAgent.Application.Regulations;
using USMAgent.Infrastructure.FileSystem;
using USMAgent.Infrastructure.FileSystem.Persistence;
using USMAgent.Infrastructure.VectorStore;

namespace USMAgent.Infrastructure.Indexing;

public sealed class RegulationIndexer
{
    private const string VectorSizeProbeText = "Text to store embending number";

    private readonly ITextEmbeddingGenerator _embeddings;
    private readonly QdrantRegulationIndexStore _store;
    private readonly ProcessedFilesStore _processedFiles;
    private readonly IndexingOptions _options;
    private readonly ILogger<RegulationIndexer> _logger;

    public RegulationIndexer(
        ITextEmbeddingGenerator embeddings,
        QdrantRegulationIndexStore store,
        ProcessedFilesStore processedFiles,
        IOptions<IndexingOptions> options,
        ILogger<RegulationIndexer> logger)
    {
        _embeddings = embeddings;
        _store = store;
        _processedFiles = processedFiles;
        _options = options.Value;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var processed = await _processedFiles.LoadAsync(cancellationToken);

        if (processed.Count == 0)
            throw new InvalidOperationException("Processed files state is empty.");

        await EnsureCollectionAsync(cancellationToken);

        _logger.LogInformation("Embedding starting.");

        var chunksRoot = ResolvePath(_options.ChunksPath);

        foreach (var filePath in Directory.EnumerateFiles(chunksRoot, "*.json", SearchOption.AllDirectories))
        {
            var sha256 = await FileHashHelper.ComputeSha256Async(filePath, cancellationToken);

            if (processed.Any(x => string.Equals(x.Sha256, sha256, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogInformation("Skipping already processed file: {File}", filePath);
                continue;
            }

            _logger.LogInformation("Uploading chunks from the file {File}", filePath);

            var uploaded = await UploadFileAsync(filePath, cancellationToken);

            _logger.LogInformation("Uploaded points: {Count}", uploaded);

            processed.Add(CreateRecord(filePath, sha256));

            await _processedFiles.SaveAsync(processed, cancellationToken);
        }
    }

    private async Task EnsureCollectionAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting of the vector size.");

        var probe = await _embeddings.GenerateAsync(VectorSizeProbeText, cancellationToken)
            ?? throw new InvalidOperationException("Failed to determine embedding vector size.");

        _logger.LogInformation("Vector size: {Size}", probe.Length);

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