using USMAgent.Application.Abstractions;
using USMAgent.Application.Models;

namespace USMAgent.Application;

public sealed class RegulationIndexer
{
    private const string VectorSizeProbeText = "Text to store embending number";

    private readonly ITextEmbeddingGenerator _embeddings;
    private readonly IRegulationIndexStore _store;
    private readonly IProcessedFilesStore _processedFiles;
    private readonly IRegulationChunkSource _source;

    public RegulationIndexer(
        ITextEmbeddingGenerator embeddings,
        IRegulationIndexStore store,
        IProcessedFilesStore processedFiles,
        IRegulationChunkSource source)
    {
        _embeddings = embeddings;
        _store = store;
        _processedFiles = processedFiles;
        _source = source;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var processed = await _processedFiles.LoadAsync(cancellationToken);

        await EnsureCollectionAsync(cancellationToken);

        foreach (var document in await _source.ListAsync(cancellationToken))
        {
            if (processed.Any(x => string.Equals(x.Sha256, document.Hash, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            await UploadAsync(document, cancellationToken);

            processed.Add(CreateRecord(document));

            await _processedFiles.SaveAsync(processed, cancellationToken);
        }
    }

    private async Task EnsureCollectionAsync(CancellationToken cancellationToken)
    {
        var probe = await _embeddings.GenerateAsync(VectorSizeProbeText, cancellationToken)
            ?? throw new InvalidOperationException("Failed to determine embedding vector size.");

        await _store.EnsureCollectionAsync((uint)probe.Length, cancellationToken);
    }

    private async Task UploadAsync(RegulationChunkDocument document, CancellationToken cancellationToken)
    {
        var chunks = await _source.ReadAsync(document, cancellationToken);

        var items = new List<(RegulationChunk, float[])>(chunks.Count);

        foreach (var chunk in chunks)
        {
            var vector = await _embeddings.GenerateAsync(chunk.EmbeddingText, cancellationToken);

            if (vector is null || vector.Length == 0)
                throw new InvalidOperationException($"Failed to embed chunk {chunk.ChunkIndex} of {document.Name}.");

            items.Add((chunk, vector));
        }

        await _store.UpsertAsync(items, cancellationToken);
    }

    private static ProcessedFileRecord CreateRecord(RegulationChunkDocument document) => new()
    {
        FileName = document.Name,
        RelativePath = document.Id,
        Sha256 = document.Hash,
        Size = document.Size,
        LastWriteTimeUtc = document.LastModifiedUtc,
        ProcessedAtUtc = DateTime.UtcNow,
        ProcesedFilename = "qdrantstorage"
    };
}
