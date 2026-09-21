using Microsoft.Extensions.Options;
using OllamaSharp;
using USMAgent.Application.Abstractions;

namespace USMAgent.Infrastructure.AI.Ollama;

public sealed class OllamaEmbeddingGenerator : ITextEmbeddingGenerator, IDisposable
{
    private readonly OllamaApiClient _client;

    public OllamaEmbeddingGenerator(
        IOptions<OllamaOptions> options)
    {
        var settings = options.Value;

        _client = new OllamaApiClient(settings.Endpoint, settings.EmbeddingModel);
        _client.DefaultRequestHeaders["Authorization"] = $"Bearer {settings.ApiKey}";
    }

    public async Task<float[]?> GenerateAsync(string text, CancellationToken cancellationToken = default)
    {
        if (!await _client.IsRunningAsync(cancellationToken))
        {
            throw new InvalidOperationException("Could not connect to USM Ollama server.");
        }

        var response = await _client.EmbedAsync(text, cancellationToken: cancellationToken);

        return response.Embeddings.FirstOrDefault();
    }

    public void Dispose() => _client.Dispose();
}