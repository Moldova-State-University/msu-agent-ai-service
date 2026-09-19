using System.Text.Encodings.Web;
using System.Text.Json;
using USMAgent.Application.Abstractions;
using USMAgent.Application.Models;

namespace USMAgent.Infrastructure.FileSystem.Json;
 public class JsonRegulationChunkStore : IRegulationChunkStore
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

    public async Task<List<RegulationChunk>> ReadAsync(string inputPath, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(inputPath);

        var chunks = await JsonSerializer.DeserializeAsync<List<RegulationChunk>>(stream,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }, cancellationToken);

        return chunks ?? [];
    }


}



