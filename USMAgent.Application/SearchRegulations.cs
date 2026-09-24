using USMAgent.Application.Abstractions;
using USMAgent.Application.Models;
using USMAgent.Domain;
using USMAgent.Domain.Enums;

namespace USMAgent.Application;

public sealed class SearchRegulations : ISearchRegulations
{
    private const int ResultsLimit = 5;

    private readonly IQueryPreprocessor _queryPreprocessor;
    private readonly ITextEmbeddingGenerator _embeddingGenerator;
    private readonly IRegulationSearchStore _searchStore;

    public SearchRegulations(
        IQueryPreprocessor queryPreprocessor,
        ITextEmbeddingGenerator embeddingGenerator,
        IRegulationSearchStore searchStore)
    {
        _queryPreprocessor = queryPreprocessor;
        _embeddingGenerator = embeddingGenerator;
        _searchStore = searchStore;
    }

    public async Task<Response<IReadOnlyList<RegulationSearchResult>>> ExecuteAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Response<IReadOnlyList<RegulationSearchResult>>.Fail(
                ErrorCode.InvalidArguments, "Query must not be empty.");
        }

        var prepared = await _queryPreprocessor.PrepareAsync(query, cancellationToken);

        // Minimal version: search by the Romanian translation when there is one, otherwise by the original text.
        var searchText = prepared.RomanianQueries.FirstOrDefault() ?? prepared.Original;

        var vector = await _embeddingGenerator.GenerateAsync(searchText, cancellationToken);

        if (vector is null || vector.Length == 0)
        {
            return Response<IReadOnlyList<RegulationSearchResult>>.Fail(
                ErrorCode.SourceUnavailable, "Failed to generate embedding for query.");
        }

        var results = await _searchStore.SearchAsync(vector, ResultsLimit, cancellationToken);

        return Response<IReadOnlyList<RegulationSearchResult>>.Ok(results);
    }
}
