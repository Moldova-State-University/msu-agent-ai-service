using USMAgent.Domain.Text;

namespace USMAgent.Infrastructure.Schedule;

/// <summary>
/// Разбор "человеческих" строковых аргументов тулов (даты, время, день недели)
/// в типизированные значения. Все методы терпимы к формату -- модель не всегда
/// пришлёт ровно dd:mm:yyyy, как написано в докстрингах тулов.
/// </summary>
internal static class ScheduleParsing
{
    // ISO 8601: понедельник = 1 ... воскресенье = 7.
    private static readonly Dictionary<string, int> DayNames = BuildDayNames();

    private static Dictionary<string, int> BuildDayNames()
    {
        var raw = new (int Iso, string[] Names)[]
        {
            (1, ["monday", "luni", "понедельник", "пн"]),
            (2, ["tuesday", "marti", "marți", "вторник", "вт"]),
            (3, ["wednesday", "miercuri", "среда", "ср"]),
            (4, ["thursday", "joi", "четверг", "чт"]),
            (5, ["friday", "vineri", "пятница", "пт"]),
            (6, ["saturday", "sambata", "sâmbătă", "суббота", "сб"]),
            (7, ["sunday", "duminica", "duminică", "воскресенье", "вс"]),
        };

        var map = new Dictionary<string, int>();
        foreach (var (iso, names) in raw)
        foreach (var name in names)
            map[TextNormalizer.Normalize(name)] = iso;

        return map;
    }

    /// <summary>Строка (любое из "Monday"/"понедельник"/"luni"/...) -> ISO день недели (1-7), либо null.</summary>
    public static int? ParseDayOfWeek(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = TextNormalizer.Normalize(value);
        return DayNames.TryGetValue(normalized, out var iso) ? iso : null;
    }

    public static int ToIsoDayOfWeek(DayOfWeek dayOfWeek) => dayOfWeek == DayOfWeek.Sunday ? 7 : (int)dayOfWeek;

    /// <summary>
    /// Разбирает дату. Основной формат по докстрингам тулов -- dd:mm:yyyy,
    /// но на всякий случай принимаются и dd.mm.yyyy / dd/mm/yyyy / dd-mm-yyyy.
    /// </summary>
    public static DateOnly? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var parts = value.Trim().Split([':', '.', '/', '-'], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
            return null;

        if (!int.TryParse(parts[0], out var day)) return null;
        if (!int.TryParse(parts[1], out var month)) return null;
        if (!int.TryParse(parts[2], out var year)) return null;

        try
        {
            return new DateOnly(year, month, day);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    /// <summary>Время в формате HH:mm или HH:mm:ss.</summary>
    public static TimeOnly? ParseTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return TimeOnly.TryParse(value.Trim(), out var time) ? time : null;
    }

    /// <summary>
    /// "dd:mm:yyyy HH:mm" -> (дата, время). Части разделены пробелом.
    /// Докстринги тулов описывают только формат даты внутри currentDateTime,
    /// формат времени не был явно зафиксирован -- принимается HH:mm[:ss].
    /// </summary>
    public static (DateOnly Date, TimeOnly Time)? ParseDateTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var tokens = value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length != 2)
            return null;

        var date = ParseDate(tokens[0]);
        var time = ParseTime(tokens[1]);

        return date is not null && time is not null ? (date.Value, time.Value) : null;
    }

    /// <summary>
    /// Номер недели (1-based) от начала периода и признак "нечётная".
    /// Неделя 1 -> нечётная (OddWeek), неделя 2 -> чётная (EvenWeek), и так далее.
    /// </summary>
    public static bool IsOddWeek(DateOnly date, DateOnly periodStart)
    {
        var daysSinceStart = date.DayNumber - periodStart.DayNumber;
        var weekIndex = (int)Math.Floor(daysSinceStart / 7.0); // 0-based, корректно и для дат до начала периода
        var weekNumber = weekIndex + 1;
        return ((weekNumber % 2) + 2) % 2 == 1;
    }

    /// <summary>
    /// Синонимы для явно переданной чётности недели: par/pară/even -> чётная,
    /// impar/impară/odd -> нечётная. Возвращает null, если распознать не удалось.
    /// </summary>
    public static bool? ParseExplicitParity(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = TextNormalizer.Normalize(value);
        return normalized switch
        {
            "par" or "para" or "even" or "evenweek" or "четная" or "чётная" => false, // false = не нечётная
            "impar" or "impara" or "odd" or "oddweek" or "нечетная" or "нечётная" => true,
            _ => null,
        };
    }

    /// <summary>Строковое значение Parity (EveryWeek/OddWeek/EvenWeek/null) подходит ли под "нечётная ли это неделя".</summary>
    public static bool MatchesParity(string? entryParity, bool? isOddWeek)
    {
        if (isOddWeek is null) return true; // чётность не уточнена -- не фильтруем
        return entryParity switch
        {
            null or "" or "EveryWeek" => true,
            "OddWeek" => isOddWeek.Value,
            "EvenWeek" => !isOddWeek.Value,
            _ => true, // неизвестное значение -- не отбрасываем агрессивно
        };
    }

    /// <summary>Суффикс имени аудитории после последнего "/" -- эвристика "блока", т.к. отдельной колонки Block нет.</summary>
    public static string? ExtractBlock(string roomName)
    {
        var idx = roomName.LastIndexOf('/');
        return idx >= 0 && idx < roomName.Length - 1 ? roomName[(idx + 1)..] : null;
    }
}
