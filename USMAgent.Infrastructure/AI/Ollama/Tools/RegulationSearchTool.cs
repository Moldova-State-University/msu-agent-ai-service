using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;
using System.Text.Json;
using USMAgent.Application.Abstractions;

namespace USMAgent.Infrastructure.AI.Ollama.Tools;

public static class RegulationSearchTool
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private static IServiceProvider? _services;

    public static void Initialize(IServiceProvider services) => _services = services;

    /// <summary>
    /// Search USM official regulations and documents by semantic meaning.
    /// Use this tool for any question about regulations, official documents, articles,
    /// student rights, student obligations, duties, requirements, procedures,
    /// normative acts, legal basis, document approval, schedules rules, exams,
    /// credits, study contracts, or university rules.
    /// If the user does not specify the exact document title, still call this tool
    /// using the user's original question.
    /// </summary>
    /// <param name="query">
    /// User question or search query. Must not be null, empty, or whitespace.
    /// Can be a full natural language question in Russian, Romanian, or English.
    /// </param>
    [OllamaTool]
    public static async Task<string> SearchRegulations(string query)
    {
        var services = _services
            ?? throw new InvalidOperationException(
                $"{nameof(RegulationSearchTool)} is not initialized. Call Initialize(IServiceProvider) at startup.");

        var search = services.GetRequiredService<ISearchRegulations>();

        try
        {
            var results = await search.ExecuteAsync(query);

            return JsonSerializer.Serialize(results, JsonOptions);
        }
        catch (ArgumentException ex)
        {
            return ToolError.InvalidArguments(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ToolError.SourceUnavailable(ex.Message);
        }
    }
}