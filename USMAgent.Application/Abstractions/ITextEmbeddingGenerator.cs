namespace USMAgent.Application.Abstractions;
public interface ITextEmbeddingGenerator
{
    Task<float[]?> GenerateAsync(string text, CancellationToken cancellationToken = default);
}