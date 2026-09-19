namespace USMAgent.Infrastructure.FileSystem;

public class PromptStore
{
    public async Task<string> ReadAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var path = Path.IsPathRooted(relativePath)
            ? relativePath
            : Path.Combine(AppContext.BaseDirectory, relativePath);

        if (!File.Exists(path))
            throw new FileNotFoundException($"Prompt file not found: {path}", path);
         
        var content = await File.ReadAllTextAsync(path, cancellationToken);

        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException($"Prompt file is empty: {path}");

        return content;
    }
}