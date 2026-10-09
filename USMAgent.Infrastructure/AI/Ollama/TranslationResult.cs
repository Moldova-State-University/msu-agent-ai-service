using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace USMAgent.Infrastructure.AI.Ollama;

public sealed class TranslationResult
{
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("query_ro")]
    public string? QueryRo { get; set; }
}

