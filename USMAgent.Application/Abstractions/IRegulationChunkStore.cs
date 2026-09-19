using USMAgent.Application.Models;

namespace USMAgent.Application.Abstractions;

public interface IRegulationChunkStore
{
    Task WriteAsync(string outputPath, IReadOnlyCollection<RegulationChunk> chunks);
    Task<List<RegulationChunk>> ReadAsync(string inputPath, CancellationToken cancellationToken = default);
}
