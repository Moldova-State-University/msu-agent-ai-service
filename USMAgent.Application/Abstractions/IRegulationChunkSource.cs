using USMAgent.Application.Models;

namespace USMAgent.Application.Abstractions;

public interface IRegulationChunkSource
{
    Task<IReadOnlyList<RegulationChunkDocument>> ListAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RegulationChunk>> ReadAsync(
        RegulationChunkDocument document,
        CancellationToken cancellationToken = default);
}
