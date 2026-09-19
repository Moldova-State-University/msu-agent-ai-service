using USMAgent.Application.Models;

namespace USMAgent.Application;

public sealed class RegulationSearchService
{
    private const int ResultsLimit = 3;

    private readonly IRegulationSearch _search;

    public RegulationSearchService(IRegulationSearch search)
    {
        _search = search;
    }

    public async Task<IReadOnlyList<RegulationSearchResult>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Query must not be empty.", nameof(query));
        }

        var results = await _search.SearchAsync(query, ResultsLimit, cancellationToken);

        return results;
    }
}