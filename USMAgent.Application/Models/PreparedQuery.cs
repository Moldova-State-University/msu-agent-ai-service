namespace USMAgent.Application.Models;

/// <summary>
/// Result of query preprocessing.
/// </summary>
/// <param name="Original">The user's question exactly as received.</param>
/// <param name="Language">Detected language of the question: "ro", "ru", "en" or "unknown".</param>
/// <param name="RomanianQueries">
/// One Romanian search query per independent part of the question (usually one, at most three).
/// Empty when the question is already Romanian or when translation failed.
/// </param>
public sealed record PreparedQuery(
    string Original,
    string Language,
    IReadOnlyList<string> RomanianQueries)
{
    /// <summary>
    /// Texts to embed and search, in priority order: Romanian queries first,
    /// then the original question as a fallback. Duplicates are removed.
    /// </summary>
    public IReadOnlyList<string> SearchTexts =>
        RomanianQueries
            .Append(Original)
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Select(text => text.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    /// <summary>No preprocessing: search runs on the original question only.</summary>
    public static PreparedQuery Passthrough(string original) =>
        new(original, "unknown", []);
}
