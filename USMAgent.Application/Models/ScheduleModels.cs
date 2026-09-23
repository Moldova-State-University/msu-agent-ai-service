namespace USMAgent.Application.Models;

/// <summary>Одна пара в расписании, готовая к сериализации для ответа тула.</summary>
public sealed record LessonDto(
    int ScheduleEntryId,
    string DayOfWeek,          
    string? Parity,            // EveryWeek | OddWeek | EvenWeek
    int TimeSlotNumber,
    string? TimeSlotStart,     
    string? TimeSlotEnd,
    string CourseName,
    string LessonType,
    string? Subgroup,
    string? Specialization,
    string? Alternative,
    string? RoomName,
    IReadOnlyList<string> Groups,
    IReadOnlyList<string> Teachers);

/// <summary>Свободная аудитория на заданный интервал.</summary>
public sealed record FreeClassroomDto(string RoomName);

/// <summary>Группа, кратко -- для GetGroupsByTeacher.</summary>
public sealed record GroupSummaryDto(string GroupName, string? Language);

/// <summary>Преподаватель, кратко -- для GetTeacherForSubject.</summary>
public sealed record TeacherSummaryDto(string FirstName, string LastName, string ShortName);
