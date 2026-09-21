using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;
using USMAgent.Application.Abstractions;
using USMAgent.Application.Models;
using USMAgent.Domain;

namespace USMAgent.Infrastructure.AI.Ollama.Tools;

public static class RegulationSearchTool
{
    private static IServiceProvider? _services;

    public static void Initialize(IServiceProvider services) => _services = services;

    /// <summary>
    /// Search USM official regulations and documents by semantic meaning.
    /// Use this tool for any question about regulations, official documents, articles,
    /// student rights, student obligations, duties, requirements, procedures,
    /// normative acts, legal basis, document approval, exams, credits, study contracts,
    /// or university rules.
    /// If the user does not name the exact document, still call this tool.
    /// The query argument must be a faithful Romanian translation of the user's question.
    /// </summary>
    /// <param name="query">
    /// One short, natural Romanian sentence or noun phrase that restates the user's question.
    /// MUST be in Romanian, because all documents are in Romanian, whatever language the user wrote in.
    /// Translate the meaning of the question faithfully and nothing more:
    /// - Do NOT add words, facts, numbers or guesses that are not in the user's question
    ///   (never put your own assumed answer into the query).
    /// - Do NOT add filler words such as "regulamente", "USM", "condiții", "proceduri",
    ///   "listă completă" and do NOT write a list of keywords.
    /// - Keep names, group codes (e.g. IA2403), article numbers and numbers exactly as given.
    /// - Use official terms: "concediu academic", "exmatriculare", "contract de studii",
    ///   "credite de studii", "transfer", "frecvență redusă", "învățământ dual", "teză de licență".
    /// If the question has several independent parts, call this tool once per part
    /// with a different query for each; never repeat the same query.
    /// Examples:
    /// "Как взять академический отпуск?" becomes "Cum se acordă concediul academic?".
    /// "Сколько кредитов нужно набрать за год?" becomes "Câte credite de studii trebuie acumulate într-un an de studii?".
    /// Bad: "numar credite studiu an minim exmatriculare 40 credite frecvență redusă".
    /// </param>
    [OllamaTool]
    public static async Task<string> SearchRegulations(string query)
    {
        Console.WriteLine($"[TOOL] SearchRegulations query = \"{query}\"");
        var services = _services
            ?? throw new InvalidOperationException(
                $"{nameof(RegulationSearchTool)} is not initialized. Call Initialize(IServiceProvider) at startup.");

        var search = services.GetRequiredService<ISearchRegulations>();

        var result = await search.ExecuteAsync(query);

        Console.WriteLine($"Chunk texts: {result.Data?.FirstOrDefault()?.ChunkText}");
        Console.WriteLine($"[TOOL] results = {result.Data?.Count}, top score = {result.Data?.FirstOrDefault()?.Score}");
        return ToolJson.Serialize(result);
    }
}
