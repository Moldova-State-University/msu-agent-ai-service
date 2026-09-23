using System.Globalization;
using System.Text;

namespace USMAgent.Domain.Text;

/// <summary>
/// Нормализует и приводит к общему виду строки на румынском/русском/английском,
/// чтобы искать по названиям предметов, преподавателей и групп независимо от
/// диакритики (ă, â, î, ș, ț) и от того, набран ли запрос кириллицей (транслитерация).
/// </summary>
public static class TextNormalizer
{
    // Транслитерация кириллицы в латиницу (упрощённая, без учёта мягкого/твёрдого знака).
    private static readonly Dictionary<char, string> CyrillicToLatin = new()
    {
        ['а'] = "a", ['б'] = "b", ['в'] = "v", ['г'] = "g", ['д'] = "d",
        ['е'] = "e", ['ё'] = "e", ['ж'] = "zh", ['з'] = "z", ['и'] = "i",
        ['й'] = "i", ['к'] = "k", ['л'] = "l", ['м'] = "m", ['н'] = "n",
        ['о'] = "o", ['п'] = "p", ['р'] = "r", ['с'] = "s", ['т'] = "t",
        ['у'] = "u", ['ф'] = "f", ['х'] = "h", ['ц'] = "c", ['ч'] = "ch",
        ['ш'] = "sh", ['щ'] = "sch", ['ъ'] = "", ['ы'] = "y", ['ь'] = "",
        ['э'] = "e", ['ю'] = "yu", ['я'] = "ya",
    };

    /// <summary>
    /// Приводит строку к нормальной форме для сравнения: нижний регистр,
    /// кириллица транслитерируется в латиницу, диакритика (ă/â/î/ș/ț и любая другая)
    /// снимается через Unicode-разложение, лишние пробелы схлопываются.
    /// </summary>
    public static string Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var transliterated = new StringBuilder(input.Length);
        foreach (var ch in input.ToLowerInvariant())
        {
            if (CyrillicToLatin.TryGetValue(ch, out var latin))
                transliterated.Append(latin);
            else
                transliterated.Append(ch);
        }

        var decomposed = transliterated.ToString().Normalize(NormalizationForm.FormD);

        var result = new StringBuilder(decomposed.Length);
        foreach (var ch in decomposed)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category == UnicodeCategory.NonSpacingMark)
                continue; // снимаем диакритические знаки (ă -> a, î -> i, ș -> s, ț -> t, ...)

            result.Append(char.IsWhiteSpace(ch) ? ' ' : ch);
        }

        // схлопнуть повторные пробелы и обрезать края
        var normalized = string.Join(' ', result.ToString()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries));

        return normalized;
    }

    /// <summary>
    /// true, если normalize(candidate) совпадает с normalize(query) полностью,
    /// содержит его как подстроку, или наоборот -- запрос содержит кандидата.
    /// Используется как первый (точный/подстрочный) уровень поиска.
    /// </summary>
    public static bool LooseContains(string candidate, string normalizedQuery)
    {
        if (string.IsNullOrEmpty(normalizedQuery))
            return false;

        var normalizedCandidate = Normalize(candidate);
        return normalizedCandidate.Contains(normalizedQuery, StringComparison.Ordinal)
            || normalizedQuery.Contains(normalizedCandidate, StringComparison.Ordinal);
    }

    /// <summary>
    /// Расстояние Левенштейна между уже нормализованными строками.
    /// Используется как запасной (нечёткий) уровень поиска, когда точных/подстрочных
    /// совпадений не нашлось -- покрывает опечатки и разницу окончаний слов
    /// при транслитерации (например "matematika" при поиске "matematica").
    /// </summary>
    public static int LevenshteinDistance(string a, string b)
    {
        if (a.Length == 0) return b.Length;
        if (b.Length == 0) return a.Length;

        var previous = new int[b.Length + 1];
        var current = new int[b.Length + 1];

        for (var j = 0; j <= b.Length; j++)
            previous[j] = j;

        for (var i = 1; i <= a.Length; i++)
        {
            current[0] = i;
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                current[j] = Math.Min(
                    Math.Min(current[j - 1] + 1, previous[j] + 1),
                    previous[j - 1] + cost);
            }

            (previous, current) = (current, previous);
        }

        return previous[b.Length];
    }
}
