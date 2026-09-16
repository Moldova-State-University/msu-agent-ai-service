using AiChatExpirement.Services;
using OllamaSharp;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Text.Json;

public static class RegulationSearch
{
    private const string CollectionName = "regulations";
    private const string EmbeddingModel = "qwen3-embedding:4b";

    //connection to Qdrant
    private static readonly QdrantClient Qdrant = new(
        host: "localhost",
        port: 6334);

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
        if (string.IsNullOrWhiteSpace(query))
        {
            Console.WriteLine("Query is empty");
            return ToolError.InvalidArguments("Query must not be empty.");
        }

        var ollamaEndpoint = new Uri(Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT") ?? throw new KeyNotFoundException());
        string ollamaApiKey = Environment.GetEnvironmentVariable("OLLAMA_API_KEY") ?? "";


        var clientEmbendding = new OllamaApiClient(ollamaEndpoint, EmbeddingModel);


        clientEmbendding.DefaultRequestHeaders["Authorization"] = $"Bearer {ollamaApiKey}";



        if (!await clientEmbendding.IsRunningAsync())
        {
            throw new InvalidOperationException("Could not connect to USM Ollama server.");
        }

        var embeddingResponse = await clientEmbendding.EmbedAsync(query);

        var vector = embeddingResponse.Embeddings.FirstOrDefault();

        if (vector is null || vector.Length == 0)
        {
            Console.WriteLine("Failed to generate embedding for query.");
            return ToolError.SourceUnavailable("Failed to generate embedding for query.");
        }
        var searchResult = await Qdrant.SearchAsync(
            collectionName: CollectionName,
            vector: vector,
            limit: 3,
            payloadSelector: true);

        var results = searchResult.Select(point => new RegulationSearchResult
        {
            Score = point.Score,
            DocumentTitle = GetPayloadString(point.Payload, "document_title"),
            HeadingPath = GetPayloadString(point.Payload, "heading_path"),
            ChunkText = GetPayloadString(point.Payload, "chunk_text"),
            SourceFile = GetPayloadString(point.Payload, "source_file"),
            Language = GetPayloadString(point.Payload, "language")
        }).ToList();

        //Console.WriteLine(JsonSerializer.Serialize(results, new JsonSerializerOptions
        //{
        //    WriteIndented = true
        //}));

        return JsonSerializer.Serialize(results, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private static string GetPayloadString(IDictionary<string, Value> payload, string key)
    {
        if (!payload.TryGetValue(key, out var value))
            return "";

        return value.KindCase switch
        {
            Value.KindOneofCase.StringValue => value.StringValue,
            Value.KindOneofCase.IntegerValue => value.IntegerValue.ToString(),
            Value.KindOneofCase.DoubleValue => value.DoubleValue.ToString(),
            Value.KindOneofCase.BoolValue => value.BoolValue.ToString(),
            _ => value.ToString()
        };
    }
}

public sealed class RegulationSearchResult
{
    public required float Score { get; init; }
    public required string DocumentTitle { get; init; }
    public required string HeadingPath { get; init; }
    public required string ChunkText { get; init; }
    public required string SourceFile { get; init; }
    public required string Language { get; init; }
}