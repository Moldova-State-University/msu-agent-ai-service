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
        var points = await _client.SearchAsync(
            collectionName: _options.CollectionName,
            vector: vector,
            limit: (ulong)limit,
            payloadSelector: true,
            cancellationToken: cancellationToken);

        return points.Select(point => new RegulationSearchResult
        {
            Score = point.Score,
            DocumentTitle = point.Payload.GetString("document_title"),
            HeadingPath = point.Payload.GetString("heading_path"),
            ChunkText = point.Payload.GetString("chunk_text"),
            SourceFile = point.Payload.GetString("source_file"),
            Language = point.Payload.GetString("language")
        }).ToList();
    }
}