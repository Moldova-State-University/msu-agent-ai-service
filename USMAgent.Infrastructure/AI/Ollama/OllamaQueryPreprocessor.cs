using Microsoft.Extensions.Options;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using USMAgent.Application.Abstractions;
using USMAgent.Application.Models;

namespace USMAgent.Infrastructure.AI.Ollama;

/// <summary>
/// Translates a user question into a Romanian search query with a single stateless Ollama request.
/// Never throws on failure: falls back to searching by the original question.
/// </summary>
public sealed class OllamaQueryPreprocessor : IQueryPreprocessor, IDisposable
{
    private const int MaxQueryLength = 300;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly OllamaApiClient _client;
    private readonly IPromptStore _prompts;
    private readonly OllamaOptions _options;

    private string? _systemPrompt;

    public OllamaQueryPreprocessor(IOptions<OllamaOptions> options, IPromptStore prompts)
    {
        _options = options.Value;
        _prompts = prompts;

        var model = string.IsNullOrWhiteSpace(_options.PreprocessorModel)
            ? _options.ChatModel
            : _options.PreprocessorModel;

        _client = new OllamaApiClient(_options.Endpoint, model);
        _client.DefaultRequestHeaders["Authorization"] = $"Bearer {_options.ApiKey}";
    }

    public async Task<PreparedQuery> PrepareAsync(
        string question,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(question))
            return PreparedQuery.Passthrough(question);

        try
        {
            _systemPrompt ??= await _prompts.ReadAsync(_options.PreprocessorPromptPath, cancellationToken);

            var request = new ChatRequest
            {
                Messages =
                [
                    new Message(ChatRole.System, _systemPrompt),
                    new Message(ChatRole.User, question)
                ],
                Format = "json",
                Think = false,
                Options = new OllamaSharp.Models.RequestOptions
                {
                    Temperature = 0,
                    NumCtx = _options.NumCtx,   // keep equal to the chat's NumCtx, otherwise Ollama may reload the model
                    NumPredict = 200
                }
            };

            var json = new StringBuilder();

            await foreach (var part in _client.ChatAsync(request, cancellationToken))
            {
                if (part?.Message?.Content is { } content)
                    json.Append(content);
            }

            var parsed = JsonSerializer.Deserialize<TranslationResult>(json.ToString(), JsonOptions);
            var query = parsed?.QueryRo?.Trim();

            if (string.IsNullOrWhiteSpace(query) || query.Length > MaxQueryLength)
            {
                Console.WriteLine($"[PREP] invalid translation, using original. Raw: {json}");
                return PreparedQuery.Passthrough(question);
            }

            var language = NormalizeLanguage(parsed?.Language);

            Console.WriteLine($"[PREP] lang={language} \"{question}\" -> \"{query}\"");

            return new PreparedQuery(question, language, [query]);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PREP] failed, using original question: {ex.Message}");
            return PreparedQuery.Passthrough(question);
        }
    }

    private static string NormalizeLanguage(string? language) => language?.Trim().ToLowerInvariant() switch
    {
        "ro" => "ro",
        "ru" => "ru",
        "en" => "en",
        _ => "unknown"
    };

    public void Dispose() => _client.Dispose();

    private sealed class TranslationResult
    {
        [JsonPropertyName("language")]
        public string? Language { get; set; }

        [JsonPropertyName("query_ro")]
        public string? QueryRo { get; set; }
    }
}
