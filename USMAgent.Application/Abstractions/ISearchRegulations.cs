using USMAgent.Application.Models;
using USMAgent.Domain;

namespace USMAgent.Application.Abstractions;

public interface ISearchRegulations
{
    Task<Response<IReadOnlyList<RegulationSearchResult>>> ExecuteAsync(
        string query,
        CancellationToken cancellationToken = default);
}
