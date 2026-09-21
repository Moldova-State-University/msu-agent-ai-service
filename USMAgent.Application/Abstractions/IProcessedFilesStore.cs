using USMAgent.Application.Models;

namespace USMAgent.Application.Abstractions;

public interface IProcessedFilesStore
{
    Task<List<ProcessedFileRecord>> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(IReadOnlyList<ProcessedFileRecord> records, CancellationToken cancellationToken = default);
}
