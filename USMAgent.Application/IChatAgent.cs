using USMAgent.Application.Models;

namespace USMAgent.Application;
public interface IChatAgent
{
    public interface IChatAgent
    {
        IAsyncEnumerable<string> AskAsync(string question, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ModelInfo>> ListModelsAsync(CancellationToken cancellationToken = default);
    }
}