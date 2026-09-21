using System.Text.Json;
using System.Text.Json.Serialization;

namespace USMAgent.Infrastructure.AI.Ollama.Tools;

internal static class ToolJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public static string Serialize<T>(T response) => JsonSerializer.Serialize(response, Options);
}
