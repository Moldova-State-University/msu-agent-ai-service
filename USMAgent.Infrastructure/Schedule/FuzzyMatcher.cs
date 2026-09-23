using USMAgent.Domain.Text;

namespace USMAgent.Infrastructure.Schedule;

internal enum MatchOutcome
{
    NotFound,
    Found,
    Ambiguous,
}

internal readonly record struct MatchResult<T>(MatchOutcome Outcome, T? Item, IReadOnlyList<T>? Candidates = null);

/// <summary>
/// Общий нечёткий поиск "одной подходящей записи" по одному или нескольким именам
/// (название курса + алиасы, ФИО преподавателя, и т.д.), терпимый к транслитерации
/// и диакритике за счёт TextNormalizer. Три уровня, от строгого к нестрогому:
///   1) точное совпадение нормализованных строк;
///   2) подстрока (в любую сторону);
///   3) расстояние Левенштейна в пределах порога.
/// На каждом уровне: 0 совпадений -> следующий уровень, 1 -> найдено,
/// больше 1 -> неоднозначно (дальше не идём, т.к. более мягкий уровень найдёт ещё больше).
/// </summary>
internal static class FuzzyMatcher
{
    public static MatchResult<T> MatchBest<T>(
        IReadOnlyList<T> items, string? query, Func<T, IEnumerable<string?>> namesSelector)
    {
        var normalizedQuery = TextNormalizer.Normalize(query);
        if (normalizedQuery.Length == 0 || items.Count == 0)
            return new MatchResult<T>(MatchOutcome.NotFound, default);

        var normalizedNames = items
            .Select(item => (Item: item, Names: namesSelector(item)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => TextNormalizer.Normalize(n!))
                .Where(n => n.Length > 0)
                .ToList()))
            .ToList();

        // 1) точное совпадение
        var exact = normalizedNames.Where(x => x.Names.Contains(normalizedQuery)).Select(x => x.Item).ToList();
        var byExact = Resolve(exact);
        if (byExact.Outcome != MatchOutcome.NotFound)
            return byExact;

        // 2) подстрока в любую сторону
        var contains = normalizedNames
            .Where(x => x.Names.Any(n => n.Contains(normalizedQuery, StringComparison.Ordinal)
                                          || normalizedQuery.Contains(n, StringComparison.Ordinal)))
            .Select(x => x.Item)
            .ToList();
        var byContains = Resolve(contains);
        if (byContains.Outcome != MatchOutcome.NotFound)
            return byContains;

        // 3) нечёткое совпадение по Левенштейну
        var threshold = Math.Max(2, (int)(normalizedQuery.Length * 0.34));
        var scored = normalizedNames
            .Select(x => (x.Item, Distance: x.Names.Count == 0
                ? int.MaxValue
                : x.Names.Min(n => TextNormalizer.LevenshteinDistance(n, normalizedQuery))))
            .Where(x => x.Distance <= threshold)
            .OrderBy(x => x.Distance)
            .ToList();

        if (scored.Count == 0)
            return new MatchResult<T>(MatchOutcome.NotFound, default);

        var best = scored[0].Distance;
        var top = scored.Where(x => x.Distance == best).Select(x => x.Item).ToList();
        return Resolve(top);
    }

    private static MatchResult<T> Resolve<T>(List<T> matches) => matches.Count switch
    {
        0 => new MatchResult<T>(MatchOutcome.NotFound, default),
        1 => new MatchResult<T>(MatchOutcome.Found, matches[0]),
        _ => new MatchResult<T>(MatchOutcome.Ambiguous, default, matches),
    };
}
