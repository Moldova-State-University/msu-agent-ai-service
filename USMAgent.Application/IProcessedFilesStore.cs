using USMAgent.Application.Models;

namespace USMAgent.Application
{
    public interface IProcessedFilesStore
    {
        Task<List<ProcessedFileRecord>> LoadAsync();
        Task SaveAsync(List<ProcessedFileRecord> records);
    }
}
