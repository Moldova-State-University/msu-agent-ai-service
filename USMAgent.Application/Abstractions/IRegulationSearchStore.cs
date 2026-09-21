using USMAgent.Application.Models;

namespace USMAgent.Application.Abstractions;
public interface IRegulationSearchStore
{
    Task<IReadOnlyList<RegulationSearchResult>> SearchAsync(
        float[] vector,
        int limit,
        CancellationToken cancellationToken = default);
}