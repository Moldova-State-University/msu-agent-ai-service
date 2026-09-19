using USMAgent.Application.Abstractions;
using USMAgent.Application.Models;

namespace USMAgent.Application;

public sealed class SearchRegulations : ISearchRegulations
{
    private const int ResultsLimit = 3;

    private readonly ITextEmbeddingGenerator _embeddingGenerator;
    private readonly IRegulationSearchStore _searchStore;

    public SearchRegulations(
        ITextEmbeddingGenerator embeddingGenerator,
        IRegulationSearchStore searchStore)
    {
        _embeddingGenerator = embeddingGenerator;
        _searchStore = searchStore;
    }

    public async Task<IReadOnlyList<RegulationSearchResult>> ExecuteAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Query must not be empty.", nameof(query));
        }

        var vector = await _embeddingGenerator.GenerateAsync(query, cancellationToken);

        if (vector is null || vector.Length == 0)
        {
            throw new InvalidOperationException("Failed to generate embedding for query.");
        }

        var results = await _searchStore.SearchAsync(vector, ResultsLimit, cancellationToken);

        return results;
    }
}