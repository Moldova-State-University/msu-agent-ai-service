using USMAgent.Application.Models;

namespace USMAgent.Application.Abstractions;
public interface IRegulationIndexStore
{
    Task EnsureCollectionAsync(uint vectorSize, CancellationToken cancellationToken = default);
    Task UpsertAsync(IReadOnlyList<(RegulationChunk Chunk, float[] Vector)> items, CancellationToken cancellationToken = default);
}