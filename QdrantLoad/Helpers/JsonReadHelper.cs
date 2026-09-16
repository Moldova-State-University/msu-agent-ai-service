using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace QdrantLoad.Helpers;
 class JsonReadHelper
{
    public async Task WriteAsync(string outputPath, IReadOnlyCollection<RegulationChunk> chunks)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        var json = JsonSerializer.Serialize(chunks, options);

        await File.WriteAllTextAsync(outputPath, json);
    }

    public async Task<List<RegulationChunk>> ReadAsync(string inputPath)
    {
        await using var stream = File.OpenRead(inputPath);

        var chunks = await JsonSerializer.DeserializeAsync<List<RegulationChunk>>(stream,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return chunks ?? [];
    }


}



public record RegulationChunk
{
    public required string Id { get; init; }
    public required string DocumentId { get; init; }
    public required string DocumentTitle { get; init; }

    public required int ChunkIndex { get; init; }

    public required List<string> Headings { get; init; }
    public required string HeadingPath { get; init; }

    public required string ChunkText { get; init; }
    public required string EmbeddingText { get; init; }

    public required string Language { get; init; }
    public string? Version { get; init; }
    public string? ValidFrom { get; init; }

    public required string SourceFile { get; init; }
    public required string Hash { get; init; }
}