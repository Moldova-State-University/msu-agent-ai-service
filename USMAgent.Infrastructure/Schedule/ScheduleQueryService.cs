using Microsoft.EntityFrameworkCore;
using USMAgent.Application.Abstractions;
using USMAgent.Application.Models;
using USMAgent.Domain;
using USMAgent.Domain.Entities.Schedule;
using USMAgent.Domain.Enums;
using USMAgent.Domain.Text;
using USMAgent.Infrastructure.Persistence;

namespace USMAgent.Infrastructure.Schedule;

/// <summary>
/// Реализация запросов к расписанию поверх ApplicationDbContext.
/// Регистрируется как Singleton (см. ServiceCollectionExtensions), поэтому получает
/// не сам DbContext (Scoped, не потокобезопасен), а IDbContextFactory -- и создаёт
/// короткоживущий DbContext на каждый вызов метода.
/// </summary>
public sealed class ScheduleQueryService : IScheduleQueryService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public ScheduleQueryService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Response> GetGroupScheduleAsync(
        string group, string date, string dayOfWeek, string weekParity,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(group))
            return Response.Fail(ErrorCode.InvalidArguments, "Group must not be empty.");

        await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var groupMatch = await MatchGroupAsync(db, group, cancellationToken);
        if (groupMatch.Response is not null) return groupMatch.Response;

        var parsedDate = ScheduleParsing.ParseDate(date);
        var period = await GetPeriodAsync(db, parsedDate, cancellationToken);
        if (period is null)
            return Response.Fail(ErrorCode.SourceUnavailable, "No academic period is configured.");

        var dayIso = ResolveDayIso(dayOfWeek, parsedDate);
        var isOdd = ResolveIsOddWeek(weekParity, parsedDate, period);

        var entries = await QueryEntries(db, period.Id)
            .Where(e => e.Groups.Any(g => g.Id == groupMatch.Item!.Id))
            .Where(e => dayIso == null || (int)e.DayOfWeek == dayIso.Value)
            .ToListAsync(cancellationToken);

        return ToLessonsResponse(entries, isOdd);
    }

    public async Task<Response> GetTeacherScheduleAsync(
        string teacherSurname, string teacherName, string date, string dayOfWeek,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(teacherSurname))
            return Response.Fail(ErrorCode.InvalidArguments, "Teacher surname must not be empty.");

        await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var teacherMatch = await MatchTeacherAsync(db, teacherSurname, teacherName, cancellationToken);
        if (teacherMatch.Response is not null) return teacherMatch.Response;

        var parsedDate = ScheduleParsing.ParseDate(date);
        var period = await GetPeriodAsync(db, parsedDate, cancellationToken);
        if (period is null)
            return Response.Fail(ErrorCode.SourceUnavailable, "No academic period is configured.");

        var dayIso = ResolveDayIso(dayOfWeek, parsedDate);
        var isOdd = parsedDate is not null ? ScheduleParsing.IsOddWeek(parsedDate.Value, period.StartDate) : (bool?)null;

        var entries = await QueryEntries(db, period.Id)
            .Where(e => e.Teachers.Any(t => t.Id == teacherMatch.Item!.Id))
            .Where(e => dayIso == null || (int)e.DayOfWeek == dayIso.Value)
            .ToListAsync(cancellationToken);

        return ToLessonsResponse(entries, isOdd);
    }

    public async Task<Response> GetSubjectScheduleAsync(
        string group, string subjectName, string subjectType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(group))
            return Response.Fail(ErrorCode.InvalidArguments, "Group must not be empty.");
        if (string.IsNullOrWhiteSpace(subjectName))
            return Response.Fail(ErrorCode.InvalidArguments, "Subject name must not be empty.");

        await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var entries = await FindGroupSubjectEntriesAsync(db, group, subjectName, subjectType, null, null, cancellationToken);
        if (entries.Response is not null) return entries.Response;

        return ToLessonsResponse(entries.Item!, isOdd: null);
    }

    public async Task<Response> GetCurrentLessonAsync(
        string group, string currentDateTime,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(group))
            return Response.Fail(ErrorCode.InvalidArguments, "Group must not be empty.");

        var parsed = ScheduleParsing.ParseDateTime(currentDateTime);
        if (parsed is null)
            return Response.Fail(ErrorCode.InvalidArguments, "currentDateTime could not be parsed.");

        await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var groupMatch = await MatchGroupAsync(db, group, cancellationToken);
        if (groupMatch.Response is not null) return groupMatch.Response;

        var (date, time) = parsed.Value;
        var period = await GetPeriodAsync(db, date, cancellationToken);
        if (period is null)
            return Response.Fail(ErrorCode.SourceUnavailable, "No academic period is configured.");

        var dayIso = ScheduleParsing.ToIsoDayOfWeek(date.DayOfWeek);
        var isOdd = ScheduleParsing.IsOddWeek(date, period.StartDate);

        var entries = await QueryEntries(db, period.Id)
            .Where(e => e.Groups.Any(g => g.Id == groupMatch.Item!.Id) && (int)e.DayOfWeek == dayIso)
            .ToListAsync(cancellationToken);

        var current = entries
            .Where(e => ScheduleParsing.MatchesParity(e.Parity?.ToString(), isOdd))
            .Where(e => e.TimeSlot.StartTime <= time && e.TimeSlot.EndTime >= time)
            .OrderBy(e => e.TimeSlot.SlotNumber)
            .Select(MapLesson)
            .ToList();

        return current.Count == 0
            ? Response.Fail(ErrorCode.NoSchedule, "No lesson is currently in progress for this group.")
            : Response.Ok(current);
    }

    public async Task<Response> GetNextLessonAsync(
        string group, string currentDateTime,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(group))
            return Response.Fail(ErrorCode.InvalidArguments, "Group must not be empty.");

        var parsed = ScheduleParsing.ParseDateTime(currentDateTime);
        if (parsed is null)
            return Response.Fail(ErrorCode.InvalidArguments, "currentDateTime could not be parsed.");

        await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var groupMatch = await MatchGroupAsync(db, group, cancellationToken);
        if (groupMatch.Response is not null) return groupMatch.Response;

        var (startDate, startTime) = parsed.Value;

        // Ищем вперёд по дням (в пределах текущей учебной недели ×2), пока не найдём ближайшее занятие.
        for (var offset = 0; offset <= 14; offset++)
        {
            var date = startDate.AddDays(offset);
            var period = await GetPeriodAsync(db, date, cancellationToken);
            if (period is null) continue;

            var dayIso = ScheduleParsing.ToIsoDayOfWeek(date.DayOfWeek);
            var isOdd = ScheduleParsing.IsOddWeek(date, period.StartDate);

            var entries = await QueryEntries(db, period.Id)
                .Where(e => e.Groups.Any(g => g.Id == groupMatch.Item!.Id) && (int)e.DayOfWeek == dayIso)
                .ToListAsync(cancellationToken);

            var candidates = entries
                .Where(e => ScheduleParsing.MatchesParity(e.Parity?.ToString(), isOdd))
                .Where(e => offset > 0 || e.TimeSlot.StartTime > startTime)
                .OrderBy(e => e.TimeSlot.SlotNumber)
                .ToList();

            if (candidates.Count > 0)
                return Response.Ok(MapLesson(candidates[0]));
        }

        return Response.Fail(ErrorCode.NoSchedule, "No upcoming lesson was found within the next two weeks.");
    }

    public async Task<Response> GetClassroomForLessonAsync(
        string group, string subjectName, string date, string dayOfWeek,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(group))
            return Response.Fail(ErrorCode.InvalidArguments, "Group must not be empty.");
        if (string.IsNullOrWhiteSpace(subjectName))
            return Response.Fail(ErrorCode.InvalidArguments, "Subject name must not be empty.");

        await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var entries = await FindGroupSubjectEntriesAsync(db, group, subjectName, "", date, dayOfWeek, cancellationToken);
        if (entries.Response is not null) return entries.Response;

        var rooms = entries.Item!
            .Where(e => e.Room is not null)
            .Select(e => e.Room!.Name)
            .Distinct()
            .OrderBy(n => n)
            .ToList();

        return rooms.Count == 0
            ? Response.Fail(ErrorCode.NoSchedule, "No classroom is assigned for this lesson.")
            : Response.Ok(rooms);
    }

    public async Task<Response> GetTeacherForSubjectAsync(
        string group, string subjectName, string subjectType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(group))
            return Response.Fail(ErrorCode.InvalidArguments, "Group must not be empty.");
        if (string.IsNullOrWhiteSpace(subjectName))
            return Response.Fail(ErrorCode.InvalidArguments, "Subject name must not be empty.");

        await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var entries = await FindGroupSubjectEntriesAsync(db, group, subjectName, subjectType, null, null, cancellationToken);
        if (entries.Response is not null) return entries.Response;

        var teachers = entries.Item!
            .SelectMany(e => e.Teachers)
            .DistinctBy(t => t.Id)
            .Select(t => new TeacherSummaryDto(t.FirstName, t.LastName, t.ShortName))
            .OrderBy(t => t.LastName)
            .ToList();

        return teachers.Count == 0
            ? Response.Fail(ErrorCode.NoSchedule, "No teacher is assigned for this lesson.")
            : Response.Ok(teachers);
    }

    public async Task<Response> GetGroupsByTeacherAsync(
        string teacherSurname, string teacherName, string subjectName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(teacherSurname))
            return Response.Fail(ErrorCode.InvalidArguments, "Teacher surname must not be empty.");

        await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var teacherMatch = await MatchTeacherAsync(db, teacherSurname, teacherName, cancellationToken);
        if (teacherMatch.Response is not null) return teacherMatch.Response;

        var period = await GetPeriodAsync(db, null, cancellationToken);
        if (period is null)
            return Response.Fail(ErrorCode.SourceUnavailable, "No academic period is configured.");

        int? courseId = null;
        if (!string.IsNullOrWhiteSpace(subjectName))
        {
            var courseMatch = await MatchCourseAsync(db, subjectName, cancellationToken);
            if (courseMatch.Response is not null) return courseMatch.Response;
            courseId = courseMatch.Item!.Id;
        }

        var entries = await QueryEntries(db, period.Id)
            .Where(e => e.Teachers.Any(t => t.Id == teacherMatch.Item!.Id))
            .Where(e => courseId == null || e.Course.Id == courseId.Value)
            .ToListAsync(cancellationToken);

        var groups = entries
            .SelectMany(e => e.Groups)
            .DistinctBy(g => g.Id)
            .Select(g => new GroupSummaryDto(g.Name, g.Language))
            .OrderBy(g => g.GroupName)
            .ToList();

        return groups.Count == 0
            ? Response.Fail(ErrorCode.NoSchedule, "No groups found for this teacher.")
            : Response.Ok(groups);
    }

    public async Task<Response> GetLessonsByClassroomAsync(
        string classroom, string block, string date, string lessonStart,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(classroom))
            return Response.Fail(ErrorCode.InvalidArguments, "Classroom must not be empty.");

        await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var roomMatch = await MatchRoomAsync(db, classroom, block, cancellationToken);
        if (roomMatch.Response is not null) return roomMatch.Response;

        var parsedDate = ScheduleParsing.ParseDate(date);
        var period = await GetPeriodAsync(db, parsedDate, cancellationToken);
        if (period is null)
            return Response.Fail(ErrorCode.SourceUnavailable, "No academic period is configured.");

        var dayIso = ResolveDayIso(null, parsedDate);
        var isOdd = parsedDate is not null ? ScheduleParsing.IsOddWeek(parsedDate.Value, period.StartDate) : (bool?)null;
        var lessonStartTime = ScheduleParsing.ParseTime(lessonStart);

        var entries = await QueryEntries(db, period.Id)
            .Where(e => e.Room!.Id == roomMatch.Item!.Id)
            .Where(e => dayIso == null || (int)e.DayOfWeek == dayIso.Value)
            .ToListAsync(cancellationToken);

        var filtered = entries
            .Where(e => ScheduleParsing.MatchesParity(e.Parity?.ToString(), isOdd))
            .Where(e => lessonStartTime == null || e.TimeSlot.StartTime == lessonStartTime.Value)
            .OrderBy(e => e.DayOfWeek)
            .ThenBy(e => e.TimeSlot.SlotNumber)
            .Select(MapLesson)
            .ToList();

        return filtered.Count == 0
            ? Response.Fail(ErrorCode.NoSchedule, "No lessons found for this classroom.")
            : Response.Ok(filtered);
    }

    public async Task<Response> GetFreeClassroomsAsync(
        string date, string lessonStart, string lessonEnd, string block,
        CancellationToken cancellationToken = default)
    {
        var parsedDate = ScheduleParsing.ParseDate(date);
        if (parsedDate is null)
            return Response.Fail(ErrorCode.InvalidArguments, "date could not be parsed.");

        var start = ScheduleParsing.ParseTime(lessonStart);
        var end = ScheduleParsing.ParseTime(lessonEnd);
        if (start is null || end is null)
            return Response.Fail(ErrorCode.InvalidArguments, "lessonStart/lessonEnd could not be parsed.");

        await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var period = await GetPeriodAsync(db, parsedDate, cancellationToken);
        if (period is null)
            return Response.Fail(ErrorCode.SourceUnavailable, "No academic period is configured.");

        var dayIso = ScheduleParsing.ToIsoDayOfWeek(parsedDate.Value.DayOfWeek);
        var isOdd = ScheduleParsing.IsOddWeek(parsedDate.Value, period.StartDate);

        var allRooms = await db.Rooms.AsNoTracking().ToListAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(block))
            allRooms = allRooms.Where(r => string.Equals(ScheduleParsing.ExtractBlock(r.Name), block, StringComparison.OrdinalIgnoreCase)).ToList();

        var busyRoomIds = await QueryEntries(db, period.Id)
            .Where(e => (int)e.DayOfWeek == dayIso && e.Room != null)
            .ToListAsync(cancellationToken);

        var busyIds = busyRoomIds
            .Where(e => ScheduleParsing.MatchesParity(e.Parity?.ToString(), isOdd))
            .Where(e => e.TimeSlot.StartTime < end.Value && e.TimeSlot.EndTime > start.Value) // пересечение интервалов
            .Select(e => e.Room!.Id)
            .ToHashSet();

        var free = allRooms
            .Where(r => !busyIds.Contains(r.Id))
            .OrderBy(r => r.Name)
            .Select(r => new FreeClassroomDto(r.Name))
            .ToList();

        return Response.Ok(free);
    }

    public async Task<Response> GetCurrentFreeClassroomsAsync(
        string date, string timeNow, string block,
        CancellationToken cancellationToken = default)
    {
        var parsedDate = ScheduleParsing.ParseDate(date);
        if (parsedDate is null)
            return Response.Fail(ErrorCode.InvalidArguments, "date could not be parsed.");

        var now = ScheduleParsing.ParseTime(timeNow);
        if (now is null)
            return Response.Fail(ErrorCode.InvalidArguments, "timeNow could not be parsed.");

        await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var period = await GetPeriodAsync(db, parsedDate, cancellationToken);
        if (period is null)
            return Response.Fail(ErrorCode.SourceUnavailable, "No academic period is configured.");

        var dayIso = ScheduleParsing.ToIsoDayOfWeek(parsedDate.Value.DayOfWeek);
        var isOdd = ScheduleParsing.IsOddWeek(parsedDate.Value, period.StartDate);

        var allRooms = await db.Rooms.AsNoTracking().ToListAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(block))
            allRooms = allRooms.Where(r => string.Equals(ScheduleParsing.ExtractBlock(r.Name), block, StringComparison.OrdinalIgnoreCase)).ToList();

        var dayEntries = await QueryEntries(db, period.Id)
            .Where(e => (int)e.DayOfWeek == dayIso && e.Room != null)
            .ToListAsync(cancellationToken);

        var busyIds = dayEntries
            .Where(e => ScheduleParsing.MatchesParity(e.Parity?.ToString(), isOdd))
            .Where(e => e.TimeSlot.StartTime <= now.Value && e.TimeSlot.EndTime >= now.Value)
            .Select(e => e.Room!.Id)
            .ToHashSet();

        var free = allRooms
            .Where(r => !busyIds.Contains(r.Id))
            .OrderBy(r => r.Name)
            .Select(r => new FreeClassroomDto(r.Name))
            .ToList();

        return Response.Ok(free);
    }

    // ------------------------------------------------------------------
    // Общие вспомогательные методы
    // ------------------------------------------------------------------

    private static IQueryable<ScheduleEntry> QueryEntries(ApplicationDbContext db, int periodId) =>
        db.ScheduleEntries
            .AsNoTracking()
            .Where(e => e.AcademicPeriod.Id == periodId)
            .Include(e => e.Course)
            .Include(e => e.Room)
            .Include(e => e.TimeSlot)
            .Include(e => e.Teachers)
            .Include(e => e.Groups);

    private static async Task<AcademicPeriod?> GetPeriodAsync(ApplicationDbContext db, DateOnly? date, CancellationToken ct)
    {
        if (date is not null)
        {
            var covering = await db.AcademicPeriods
                .Where(p => p.StartDate <= date.Value && p.EndDate >= date.Value)
                .OrderByDescending(p => p.StartDate)
                .FirstOrDefaultAsync(ct);
            if (covering is not null) return covering;
        }

        // Нет подходящей по дате даты (например, EndDate ещё не заполнен реальным значением) --
        // берём последний начавшийся период как разумный дефолт.
        return await db.AcademicPeriods.OrderByDescending(p => p.StartDate).FirstOrDefaultAsync(ct);
    }

    private static int? ResolveDayIso(string? dayOfWeek, DateOnly? date)
    {
        return ScheduleParsing.ParseDayOfWeek(dayOfWeek)
            ?? (date is not null ? ScheduleParsing.ToIsoDayOfWeek(date.Value.DayOfWeek) : null);
    }

    private static bool? ResolveIsOddWeek(string? weekParity, DateOnly? date, AcademicPeriod period)
    {
        return ScheduleParsing.ParseExplicitParity(weekParity)
            ?? (date is not null ? ScheduleParsing.IsOddWeek(date.Value, period.StartDate) : (bool?)null);
    }

    private static Response ToLessonsResponse(List<ScheduleEntry> entries, bool? isOdd)
    {
        var lessons = entries
            .Where(e => ScheduleParsing.MatchesParity(e.Parity?.ToString(), isOdd))
            .OrderBy(e => e.DayOfWeek)
            .ThenBy(e => e.TimeSlot.SlotNumber)
            .Select(MapLesson)
            .ToList();

        return lessons.Count == 0
            ? Response.Fail(ErrorCode.NoSchedule, "No lessons found for the given filters.")
            : Response.Ok(lessons);
    }

    private static LessonDto MapLesson(ScheduleEntry e) => new(
        e.Id,
        e.DayOfWeek.ToString(),
        e.Parity?.ToString(),
        e.TimeSlot.SlotNumber,
        e.TimeSlot.StartTime.ToString("HH:mm"),
        e.TimeSlot.EndTime.ToString("HH:mm"),
        e.Course.Name,
        e.LessonType.GetValueOrDefault().ToString(),
        e.Subgroup,
        e.Specialization,
        e.Alternative,
        e.Room?.Name,
        e.Groups.Select(g => g.Name).OrderBy(n => n).ToList(),
        e.Teachers.Select(t => t.ShortName).OrderBy(n => n).ToList());

    /// <summary>Находит записи расписания группы по предмету, с опциональными типом/датой/днём недели.</summary>
    private async Task<(List<ScheduleEntry>? Item, Response? Response)> FindGroupSubjectEntriesAsync(
        ApplicationDbContext db, string group, string subjectName, string? subjectType,
        string? date, string? dayOfWeek, CancellationToken ct)
    {
        var groupMatch = await MatchGroupAsync(db, group, ct);
        if (groupMatch.Response is not null) return (null, groupMatch.Response);

        var courseMatch = await MatchCourseAsync(db, subjectName, ct);
        if (courseMatch.Response is not null) return (null, courseMatch.Response);

        var parsedDate = ScheduleParsing.ParseDate(date);
        var period = await GetPeriodAsync(db, parsedDate, ct);
        if (period is null)
            return (null, Response.Fail(ErrorCode.SourceUnavailable, "No academic period is configured."));

        var dayIso = ResolveDayIso(dayOfWeek, parsedDate);
        var normalizedType = string.IsNullOrWhiteSpace(subjectType)
            ? null
            : TextNormalizer.Normalize(subjectType);

        var entries = await QueryEntries(db, period.Id)
            .Where(e => e.Groups.Any(g => g.Id == groupMatch.Item!.Id) && e.Course.Id == courseMatch.Item!.Id)
            .Where(e => dayIso == null || (int)e.DayOfWeek == dayIso.Value)
            .ToListAsync(ct);

        if (normalizedType is not null)
        {
            entries = entries
                .Where(e => TextNormalizer.Normalize(e.LessonType.ToString()) == normalizedType)
                .ToList();
        }

        return (entries, null);
    }

    private static async Task<(Group? Item, Response? Response)> MatchGroupAsync(
        ApplicationDbContext db, string group, CancellationToken ct)
    {
        var groups = await db.Groups.AsNoTracking().ToListAsync(ct);
        var match = FuzzyMatcher.MatchBest(groups, group, g => [g.Name]);
        return match.Outcome switch
        {
            MatchOutcome.Found => (match.Item, null),
            MatchOutcome.Ambiguous => (null, Response.Fail(ErrorCode.AmbiguousName,
                $"'{group}' matches several groups: {string.Join(", ", match.Candidates!.Select(g => g.Name))}.")),
            _ => (null, Response.Fail(ErrorCode.NotFound, $"Group '{group}' was not found.")),
        };
    }

    private static async Task<(Teacher? Item, Response? Response)> MatchTeacherAsync(
        ApplicationDbContext db, string teacherSurname, string? teacherName, CancellationToken ct)
    {
        var teachers = await db.Teachers.AsNoTracking().ToListAsync(ct);
        var query = string.IsNullOrWhiteSpace(teacherName) ? teacherSurname : $"{teacherSurname} {teacherName}";

        var match = FuzzyMatcher.MatchBest(teachers, query, t =>
            [t.LastName, t.FirstName, t.ShortName, $"{t.FirstName} {t.LastName}", $"{t.LastName} {t.FirstName}"]);

        return match.Outcome switch
        {
            MatchOutcome.Found => (match.Item, null),
            MatchOutcome.Ambiguous => (null, Response.Fail(ErrorCode.AmbiguousName,
                $"'{teacherSurname}' matches several teachers: {string.Join(", ", match.Candidates!.Select(t => t.ShortName))}.")),
            _ => (null, Response.Fail(ErrorCode.NotFound, $"Teacher '{teacherSurname}' was not found.")),
        };
    }

    private static async Task<(Course? Item, Response? Response)> MatchCourseAsync(
        ApplicationDbContext db, string subjectName, CancellationToken ct)
    {
        var courses = await db.Courses.Include(c => c.Aliases).AsNoTracking().ToListAsync(ct);
        var match = FuzzyMatcher.MatchBest(courses, subjectName, c =>
            new[] { c.Name }.Concat(c.Aliases.Select(a => a.Alias)));

        return match.Outcome switch
        {
            MatchOutcome.Found => (match.Item, null),
            MatchOutcome.Ambiguous => (null, Response.Fail(ErrorCode.AmbiguousName,
                $"'{subjectName}' matches several subjects: {string.Join(", ", match.Candidates!.Select(c => c.Name))}.")),
            _ => (null, Response.Fail(ErrorCode.NotFound, $"Subject '{subjectName}' was not found.")),
        };
    }

    private static async Task<(Room? Item, Response? Response)> MatchRoomAsync(
        ApplicationDbContext db, string classroom, string? block, CancellationToken ct)
    {
        var rooms = await db.Rooms.AsNoTracking().ToListAsync(ct);
        var match = FuzzyMatcher.MatchBest(rooms, classroom, r => [r.Name]);

        if (match.Outcome == MatchOutcome.Ambiguous && !string.IsNullOrWhiteSpace(block))
        {
            var narrowed = match.Candidates!
                .Where(r => string.Equals(ScheduleParsing.ExtractBlock(r.Name), block, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (narrowed.Count == 1)
                return (narrowed[0], null);
        }

        return match.Outcome switch
        {
            MatchOutcome.Found => (match.Item, null),
            MatchOutcome.Ambiguous => (null, Response.Fail(ErrorCode.AmbiguousName,
                $"'{classroom}' matches several rooms: {string.Join(", ", match.Candidates!.Select(r => r.Name))}.")),
            _ => (null, Response.Fail(ErrorCode.NotFound, $"Classroom '{classroom}' was not found.")),
        };
    }
}
