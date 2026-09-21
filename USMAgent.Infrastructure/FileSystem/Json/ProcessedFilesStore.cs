using System.Text.Json;
using USMAgent.Application.Abstractions;
using USMAgent.Application.Models;

namespace USMAgent.Infrastructure.FileSystem.Json;

public sealed class ProcessedFilesStore : IProcessedFilesStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;

    public ProcessedFilesStore(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<List<ProcessedFileRecord>> LoadAsync(CancellationToken cancellationToken = default)
    {
        EnsureDirectory();

        if (!File.Exists(_filePath))
        {
            await SaveAsync([], cancellationToken);
            return [];
        }

        try
        {
            var json = await File.ReadAllTextAsync(_filePath, cancellationToken);

            if (string.IsNullOrWhiteSpace(json))
            {
                return [];
            }

            return JsonSerializer.Deserialize<List<ProcessedFileRecord>>(json, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
        catch (IOException)
        {
            return [];
        }
    }

    public async Task SaveAsync(IReadOnlyList<ProcessedFileRecord> records, CancellationToken cancellationToken = default)
    {
        EnsureDirectory();
        var json = JsonSerializer.Serialize(records, JsonOptions);
        await File.WriteAllTextAsync(_filePath, json, cancellationToken);
    }

    private void EnsureDirectory()
    {
        var directoryPath = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }
}
