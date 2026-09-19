namespace USMAgent.Application.Abstractions;
public interface IPromptStore
{ 
    Task<string> ReadAsync(string relativePath, CancellationToken cancellationToken = default);
}