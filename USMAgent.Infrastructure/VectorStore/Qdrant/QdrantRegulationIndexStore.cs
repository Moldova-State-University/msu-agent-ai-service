using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using USMAgent.Application.Abstractions;
using USMAgent.Application.Models;
using USMAgent.Infrastructure.VectorStore.Qdrant;

namespace USMAgent.Infrastructure.VectorStore.Qdrant;

public sealed class QdrantRegulationIndexStore : IRegulationIndexStore
{
    private readonly QdrantClient _qdrant;
    private readonly QdrantOptions _options;

    public QdrantRegulationIndexStore(QdrantClient qdrant, IOptions<QdrantOptions> options)
    {
        _qdrant = qdrant;
        _options = options.Value;
    }

    public async Task EnsureCollectionAsync(uint vectorSize, CancellationToken cancellationToken = default)
    {
        if (await _qdrant.CollectionExistsAsync(_options.CollectionName, cancellationToken))
            return;

        await _qdrant.CreateCollectionAsync(
            _options.CollectionName,
            new VectorParams { Size = vectorSize, Distance = Distance.Cosine },
            cancellationToken: cancellationToken);
    }

    public Task UpsertAsync(
        IReadOnlyList<(RegulationChunk Chunk, float[] Vector)> items,
        CancellationToken cancellationToken = default)
    {
        var points = items.Select(item => ToPoint(item.Chunk, item.Vector)).ToList();

        return _qdrant.UpsertAsync(_options.CollectionName, points, cancellationToken: cancellationToken);
    }

    private static PointStruct ToPoint(RegulationChunk chunk, float[] vector)
    {
        var point = new PointStruct
        {
            Id = Guid.NewGuid(),
            Vectors = vector
        };

        point.Payload[QdrantPayloadKeys.DocumentId] = chunk.DocumentId;
        point.Payload[QdrantPayloadKeys.DocumentTitle] = chunk.DocumentTitle;
        point.Payload[QdrantPayloadKeys.ChunkIndex] = chunk.ChunkIndex;
        point.Payload[QdrantPayloadKeys.HeadingPath] = chunk.HeadingPath;
        point.Payload[QdrantPayloadKeys.ChunkText] = chunk.ChunkText;
        point.Payload[QdrantPayloadKeys.Language] = chunk.Language;
        point.Payload[QdrantPayloadKeys.Version] = chunk.Version ?? "";
        point.Payload[QdrantPayloadKeys.ValidFrom] = chunk.ValidFrom ?? "";
        point.Payload[QdrantPayloadKeys.SourceFile] = chunk.SourceFile;
        point.Payload[QdrantPayloadKeys.Hash] = chunk.Hash;

        return point;
    }
}