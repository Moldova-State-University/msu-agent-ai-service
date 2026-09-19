using USMAgent.Application.Models;

namespace USMAgent.Application;

public interface ISearchRegulations
{
    Task<IReadOnlyList<RegulationSearchResult>> ExecuteAsync(
        string query,
        CancellationToken cancellationToken = default);
}