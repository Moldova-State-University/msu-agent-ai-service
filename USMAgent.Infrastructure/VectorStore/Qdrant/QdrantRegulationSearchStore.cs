using Microsoft.Extensions.Options;
using Qdrant.Client;
using USMAgent.Application.Abstractions;
using USMAgent.Application.Models;

namespace USMAgent.Infrastructure.VectorStore.Qdrant;

public sealed class QdrantRegulationSearchStore : IRegulationSearchStore
{
    private readonly QdrantClient _client;
    private readonly QdrantOptions _options;

    public QdrantRegulationSearchStore(QdrantClient client, IOptions<QdrantOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<RegulationSearchResult>> SearchAsync(
        float[] vector,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var points = await _client.QueryAsync(
            collectionName: _options.CollectionName,
            query: vector,
            limit: (ulong)limit,
            payloadSelector: true,
            cancellationToken: cancellationToken);

        return points.Select(point => new RegulationSearchResult
        {
            Score = point.Score,
            DocumentTitle = point.Payload.GetString(QdrantPayloadKeys.DocumentTitle),
            HeadingPath = point.Payload.GetString(QdrantPayloadKeys.HeadingPath),
            ChunkText = point.Payload.GetString(QdrantPayloadKeys.ChunkText),
            SourceFile = point.Payload.GetString(QdrantPayloadKeys.SourceFile),
            Language = point.Payload.GetString(QdrantPayloadKeys.Language)
        }).ToList();
    }
}