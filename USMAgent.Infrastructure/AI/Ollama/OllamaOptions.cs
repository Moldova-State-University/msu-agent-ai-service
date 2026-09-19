using System.ComponentModel.DataAnnotations;

namespace USMAgent.Infrastructure.AI.Ollama;

public sealed class OllamaOptions
{
    public const string SectionName = "Ollama";

    [Required]
    public required Uri Endpoint { get; init; }

    public string ApiKey { get; init; } = string.Empty;

    [Required]
    public string ChatModel { get; init; } = "qwen3.5:latest";

    [Required]
    public string EmbeddingModel { get; init; } = "qwen3-embedding:4b";
    public string SystemPromptPath { get; init; } = "Storage/Prompts/Chat/ChatBasePrompt.md";
    public bool Think { get; init; }
    public bool AllowRecursiveToolCalls { get; init; } = true;
    public float Temperature { get; init; } = 0.2f;
    public float TopP { get; init; } = 0.8f;
    public int NumCtx { get; init; } = 8192;
}
