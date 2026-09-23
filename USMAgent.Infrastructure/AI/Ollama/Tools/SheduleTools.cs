using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;
using USMAgent.Application.Abstractions;
using USMAgent.Domain;
using USMAgent.Domain.Enums;

namespace USMAgent.Infrastructure.AI.Ollama.Tools;

public static class ScheduleTools
{
    private static IServiceProvider? _services;

    /// <summary>Вызывать один раз при старте приложения, рядом с RegulationSearchTool.Initialize.</summary>
    public static void Initialize(IServiceProvider services) => _services = services;

    private static IScheduleQueryService Service =>
        (_services ?? throw new InvalidOperationException(
            $"{nameof(ScheduleTools)} is not initialized. Call Initialize(IServiceProvider) at startup."))
        .GetRequiredService<IScheduleQueryService>();

    /// <summary>
    /// Get the schedule of a student group for a specific date, day of week, or week.
    /// Date format: dd:mm:yyyy.
    /// </summary>
    /// <param name="group">Student group name, for example IA2403</param>
    /// <param name="date">Specific date in format dd:mm:yyyy, or empty whitespace if not specified</param>
    /// <param name="dayOfWeek">Day of week, or empty whitespace if not specified</param>
    /// <param name="weekParity">Week parity: par, impar, or empty whitespace if not specified</param>
    [OllamaTool]
    public static async Task<string> GetGroupSchedule(string group, string date = "", string dayOfWeek = "", string weekParity = "")
    {
        var result = await Service.GetGroupScheduleAsync(group, date, dayOfWeek, weekParity);
        return ToolJson.Serialize(result);
    }

    /// <summary>
    /// Get the schedule of a teacher.
    /// Date format: dd:mm:yyyy.
    /// </summary>
    /// <param name="teacherSurname">Teacher surname</param>
    /// <param name="teacherName">Teacher first name, or empty whitespace if not specified</param>
    /// <param name="date">Specific date in format dd:mm:yyyy, or empty whitespace if not specified</param>
    /// <param name="dayOfWeek">Day of week, or empty whitespace if not specified</param>
    [OllamaTool]
    public static async Task<string> GetTeacherSchedule(string teacherSurname, string teacherName = "", string date = "", string dayOfWeek = "")
    {
        var result = await Service.GetTeacherScheduleAsync(teacherSurname, teacherName, date, dayOfWeek);
        return ToolJson.Serialize(result);
    }

    /// <summary>
    /// Get the schedule of a specific subject for a student group.
    /// </summary>
    /// <param name="group">Student group name, for example IA2403</param>
    /// <param name="subjectName">Subject name</param>
    /// <param name="subjectType">Lesson type: curs, lab, seminar, or empty whitespace if not specified</param>
    [OllamaTool]
    public static async Task<string> GetSubjectSchedule(string group, string subjectName, string subjectType = "")
    {
        var result = await Service.GetSubjectScheduleAsync(group, subjectName, subjectType);
        return ToolJson.Serialize(result);
    }

    /// <summary>
    /// Get the current lesson of a student group.
    /// Date format inside currentDateTime: dd:mm:yyyy HH:mm.
    /// </summary>
    /// <param name="group">Student group name, for example IA2403</param>
    /// <param name="currentDateTime">Current date and time, using format "dd:mm:yyyy HH:mm"</param>
    [OllamaTool]
    public static async Task<string> GetCurrentLesson(string group, string currentDateTime)
    {
        var result = await Service.GetCurrentLessonAsync(group, currentDateTime);
        return ToolJson.Serialize(result);
    }

    /// <summary>
    /// Get the next lesson of a student group after the current date and time.
    /// Date format inside currentDateTime: dd:mm:yyyy HH:mm.
    /// </summary>
    /// <param name="group">Student group name, for example IA2403</param>
    /// <param name="currentDateTime">Current date and time, using format "dd:mm:yyyy HH:mm"</param>
    [OllamaTool]
    public static async Task<string> GetNextLesson(string group, string currentDateTime)
    {
        var result = await Service.GetNextLessonAsync(group, currentDateTime);
        return ToolJson.Serialize(result);
        
    }

    /// <summary>
    /// Get the classroom for a specific subject or lesson.
    /// Date format: dd:mm:yyyy.
    /// </summary>
    /// <param name="group">Student group name, for example IA2403</param>
    /// <param name="subjectName">Subject name</param>
    /// <param name="date">Specific date in format dd:mm:yyyy, or empty whitespace if not specified</param>
    /// <param name="dayOfWeek">Day of week, or empty whitespace if not specified</param>
    [OllamaTool]
    public static async Task<string> GetClassroomForLesson(string group, string subjectName, string date = "", string dayOfWeek = "")
    {
        var result = await Service.GetClassroomForLessonAsync(group, subjectName, date, dayOfWeek);
        return ToolJson.Serialize(result);
    }

    /// <summary>
    /// Get the teacher who teaches a specific subject for a student group.
    /// </summary>
    /// <param name="group">Student group name, for example IA2403</param>
    /// <param name="subjectName">Subject name</param>
    /// <param name="subjectType">Lesson type: curs, lab, seminar, or empty whitespace if not specified</param>
    [OllamaTool]
    public static async Task<string> GetTeacherForSubject(string group, string subjectName, string subjectType = "")
    {
        var result = await Service.GetTeacherForSubjectAsync(group, subjectName, subjectType);
        return ToolJson.Serialize(result);
    }

    /// <summary>
    /// Get all groups taught by a specific teacher.
    /// </summary>
    /// <param name="teacherSurname">Teacher surname</param>
    /// <param name="teacherName">Teacher first name, or empty whitespace if not specified</param>
    /// <param name="subjectName">Subject name, or empty whitespace if not specified</param>
    [OllamaTool]
    public static async Task<string> GetGroupsByTeacher(string teacherSurname, string teacherName = "", string subjectName = "")
    {
        var result = await Service.GetGroupsByTeacherAsync(teacherSurname, teacherName, subjectName);
        return ToolJson.Serialize(result);
    }

    /// <summary>
    /// Get lessons taking place in a specific classroom.
    /// Date format: dd:mm:yyyy.
    /// </summary>
    /// <param name="classroom">Classroom number</param>
    /// <param name="block">University block, or empty whitespace if not specified</param>
    /// <param name="date">Specific date in format dd:mm:yyyy, or empty whitespace if not specified</param>
    /// <param name="lessonStart">Lesson start time (HH:mm), or empty whitespace if not specified</param>
    [OllamaTool]
    public static async Task<string> GetLessonsByClassroom(string classroom, string block = "", string date = "", string lessonStart = "")
    {
        var result = await Service.GetLessonsByClassroomAsync(classroom, block, date, lessonStart);
        return ToolJson.Serialize(result);
    }

    /// <summary>
    /// Get free classrooms for a specific time interval.
    /// Date format: dd:mm:yyyy.
    /// </summary>
    /// <param name="date">Specific date in format dd:mm:yyyy</param>
    /// <param name="lessonStart">Start time of the requested interval (HH:mm)</param>
    /// <param name="lessonEnd">End time of the requested interval (HH:mm)</param>
    /// <param name="block">University block, or empty whitespace if not specified</param>
    [OllamaTool]
    public static async Task<string> GetFreeClassrooms(string date, string lessonStart, string lessonEnd, string block = "")
    {
        var result = await Service.GetFreeClassroomsAsync(date, lessonStart, lessonEnd, block);
        return ToolJson.Serialize(result);
    }

    /// <summary>
    /// Get classrooms that are free right now.
    /// Date format: dd:mm:yyyy.
    /// </summary>
    /// <param name="date">Current date in format dd:mm:yyyy</param>
    /// <param name="timeNow">Current time (HH:mm)</param>
    /// <param name="block">University block, or empty whitespace if not specified</param>
    [OllamaTool]
    public static async Task<string> GetCurrentFreeClassrooms(string date, string timeNow, string block = "")
    {
        var result = await Service.GetCurrentFreeClassroomsAsync(date, timeNow, block);
        return ToolJson.Serialize(result);
    }
}
