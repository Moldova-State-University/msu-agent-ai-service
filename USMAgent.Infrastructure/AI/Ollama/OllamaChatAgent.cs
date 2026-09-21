using Microsoft.Extensions.Options;
using OllamaSharp;
using OllamaSharp.Models;
using System.Runtime.CompilerServices;
using USMAgent.Application.Abstractions;
using USMAgent.Application.Models;
using USMAgent.Infrastructure.AI.Ollama.Tools;
using USMAgent.Infrastructure.FileSystem;
using ModelInfo = USMAgent.Application.Models.ModelInfo;

namespace USMAgent.Infrastructure.AI.Ollama;

public sealed class OllamaChatAgent : IChatAgent, IDisposable
{
    private static readonly object[] AgentTools =
    [
        new SearchRegulationsTool(),
        new GetCurrentLessonTool()
    ];

    private readonly OllamaApiClient _client;
    private readonly IPromptStore _prompts;
    private readonly OllamaOptions _options;

    private OllamaSharp.Chat? _chat;

    public OllamaChatAgent(IOptions<OllamaOptions> options, IPromptStore prompts)
    {
        _options = options.Value;
        _prompts = prompts;

        _client = new OllamaApiClient(_options.Endpoint, _options.ChatModel);
        _client.DefaultRequestHeaders["Authorization"] = $"Bearer {_options.ApiKey}";
    }

    public async IAsyncEnumerable<string> AskAsync(
        string question,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var chat = await EnsureChatAsync(cancellationToken);

        await foreach (var token in chat.SendAsync(question, AgentTools, cancellationToken: cancellationToken))
        {
            yield return token;
        }
    }

    public async Task<IReadOnlyList<ModelInfo>> ListModelsAsync(CancellationToken cancellationToken = default)
    {
        IEnumerable<Model> models = await _client.ListLocalModelsAsync(cancellationToken);

        return models
            .OrderBy(m => m.Name)
            .Select(m => new ModelInfo(m.Name, m.Details.ParameterSize, m.Details.Format))
            .ToList();
    }

    private async Task<OllamaSharp.Chat> EnsureChatAsync(CancellationToken cancellationToken)
    {
        if (_chat is not null) return _chat;

        if (!await _client.IsRunningAsync(cancellationToken))
            throw new InvalidOperationException("Could not connect to USM Ollama server.");

        var systemPrompt = await _prompts.ReadAsync(_options.SystemPromptPath, cancellationToken);

        return _chat = new OllamaSharp.Chat(_client, systemPrompt)
        {
            Think = _options.Think,
            AllowRecursiveToolCalls = _options.AllowRecursiveToolCalls,
            Options = new RequestOptions
            {
                Temperature = _options.Temperature,
                TopP = _options.TopP,
                NumCtx = _options.NumCtx
            }
        };
    }

    public void Dispose() => _client.Dispose();
}