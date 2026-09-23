using USMAgent.Domain;

namespace USMAgent.Application.Abstractions;

/// <summary>
/// Оркестрация запросов к расписанию. Реализация лежит в Infrastructure
/// (работает с ApplicationDbContext), сюда наружу отдаются только Response с DTO.
/// Каждый метод соответствует одному [OllamaTool] в ScheduleTools 1:1.
/// </summary>
public interface IScheduleQueryService
{
    Task<Response> GetGroupScheduleAsync(
        string group, string date, string dayOfWeek, string weekParity,
        CancellationToken cancellationToken = default);

    Task<Response> GetTeacherScheduleAsync(
        string teacherSurname, string teacherName, string date, string dayOfWeek,
        CancellationToken cancellationToken = default);

    Task<Response> GetSubjectScheduleAsync(
        string group, string subjectName, string subjectType,
        CancellationToken cancellationToken = default);

    Task<Response> GetCurrentLessonAsync(
        string group, string currentDateTime,
        CancellationToken cancellationToken = default);

    Task<Response> GetNextLessonAsync(
        string group, string currentDateTime,
        CancellationToken cancellationToken = default);

    Task<Response> GetClassroomForLessonAsync(
        string group, string subjectName, string date, string dayOfWeek,
        CancellationToken cancellationToken = default);

    Task<Response> GetTeacherForSubjectAsync(
        string group, string subjectName, string subjectType,
        CancellationToken cancellationToken = default);

    Task<Response> GetGroupsByTeacherAsync(
        string teacherSurname, string teacherName, string subjectName,
        CancellationToken cancellationToken = default);

    Task<Response> GetLessonsByClassroomAsync(
        string classroom, string block, string date, string lessonStart,
        CancellationToken cancellationToken = default);

    Task<Response> GetFreeClassroomsAsync(
        string date, string lessonStart, string lessonEnd, string block,
        CancellationToken cancellationToken = default);

    Task<Response> GetCurrentFreeClassroomsAsync(
        string date, string timeNow, string block,
        CancellationToken cancellationToken = default);
}
