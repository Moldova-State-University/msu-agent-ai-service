using System;
using System.Collections.Generic;
using System.Text;
using OllamaSharp;
namespace USMAgent.Infrastructure.AI.Ollama.Tools;

public static class ScheduleTools
{
    /// <summary>
    /// Get the schedule of a student group for a specific date, day of week, or week.
    /// Date format: dd:mm:yyyy.
    /// </summary>
    /// <param name="group">Student group name, for example IA2403</param>
    /// <param name="date">Specific date in format dd:mm:yyyy, or empty whitespace if not specified</param>
    /// <param name="dayOfWeek">Day of week, or empty whitespace if not specified</param>
    /// <param name="weekParity">Week parity: par, impar, or empty whitespace if not specified</param>
    [OllamaTool]
    public static string GetGroupSchedule(string group, string date = "", string dayOfWeek = "", string weekParity = "")
    {
        return ToolError.InvalidArguments("Wrong agrument");
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
    public static string GetTeacherSchedule(string teacherSurname, string teacherName = "", string date = "", string dayOfWeek = "")
    {
        return ToolError.InvalidArguments("Wrong agrument");
    }

    /// <summary>
    /// Get the schedule of a specific subject for a student group.
    /// </summary>
    /// <param name="group">Student group name, for example IA2403</param>
    /// <param name="subjectName">Subject name</param>
    /// <param name="subjectType">Lesson type: curs, lab, consult, or empty whitespace if not specified</param>
    [OllamaTool]
    public static string GetSubjectSchedule(string group, string subjectName, string subjectType = "")
    {
        return ToolError.InvalidArguments("Wrong agrument");
    }

    /// <summary>
    /// Get the current lesson of a student group.
    /// Date format inside currentDateTime: dd:mm:yyyy.
    /// </summary>
    /// <param name="group">Student group name, for example IA2403</param>
    /// <param name="currentDateTime">Current date and time, using date format dd:mm:yyyy</param>
    [OllamaTool]
    public static string GetCurrentLesson(string group, string currentDateTime)
    {
        return ToolError.InvalidArguments("Wrong agruments");
    }

    /// <summary>
    /// Get the next lesson of a student group after the current date and time.
    /// Date format inside currentDateTime: dd:mm:yyyy.
    /// </summary>
    /// <param name="group">Student group name, for example IA2403</param>
    /// <param name="currentDateTime">Current date and time, using date format dd:mm:yyyy</param>
    [OllamaTool]
    public static string GetNextLesson(string group, string currentDateTime)
    {
        //тут код который ищет в базе данных следующий урок
        return ToolError.InvalidArguments("Wrong agrument");
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
    public static string GetClassroomForLesson(string group, string subjectName, string date = "", string dayOfWeek = "")
    {
        return ToolError.InvalidArguments("Wrong agrument");
    }

    /// <summary>
    /// Get the teacher who teaches a specific subject for a student group.
    /// </summary>
    /// <param name="group">Student group name, for example IA2403</param>
    /// <param name="subjectName">Subject name</param>
    /// <param name="subjectType">Lesson type: curs, lab, consult, or empty whitespace if not specified</param>
    [OllamaTool]
    public static string GetTeacherForSubject(string group, string subjectName, string subjectType = "")
    {
        return ToolError.InvalidArguments("Wrong agrument");
    }

    /// <summary>
    /// Get all groups taught by a specific teacher.
    /// </summary>
    /// <param name="teacherSurname">Teacher surname</param>
    /// <param name="teacherName">Teacher first name, or empty whitespace if not specified</param>
    /// <param name="subjectName">Subject name, or empty whitespace if not specified</param>
    [OllamaTool]
    public static string GetGroupsByTeacher(string teacherSurname, string teacherName = "", string subjectName = "")
    {
        return ToolError.InvalidArguments("Wrong agrument");
    }

    /// <summary>
    /// Get lessons taking place in a specific classroom.
    /// Date format: dd:mm:yyyy.
    /// </summary>
    /// <param name="classroom">Classroom number</param>
    /// <param name="block">University block, or empty whitespace if not specified</param>
    /// <param name="date">Specific date in format dd:mm:yyyy, or empty whitespace if not specified</param>
    /// <param name="lessonStart">Lesson start time, or empty whitespace if not specified</param>
    [OllamaTool]
    public static string GetLessonsByClassroom(string classroom, string block = "", string date = "", string lessonStart = "")
    {
        return ToolError.InvalidArguments("Wrong agrument");
    }

    /// <summary>
    /// Get free classrooms for a specific time interval.
    /// Date format: dd:mm:yyyy.
    /// </summary>
    /// <param name="date">Specific date in format dd:mm:yyyy</param>
    /// <param name="lessonStart">Start time of the requested interval</param>
    /// <param name="lessonEnd">End time of the requested interval</param>
    /// <param name="block">University block, or empty whitespace if not specified</param>
    [OllamaTool]
    public static string GetFreeClassrooms(string date, string lessonStart, string lessonEnd, string block = "")
    {
        return ToolError.InvalidArguments("Wrong agrument");
    }
    /// <summary>
    /// Get classrooms that are free right now.
    /// Date format: dd:mm:yyyy.
    /// </summary>
    /// <param name="date">Current date in format dd:mm:yyyy</param>
    /// <param name="timeNow">Current time</param>
    /// <param name="block">University block, or empty whitespace if not specified</param>
    [OllamaTool]
    public static string GetCurrentFreeClassrooms(string date, string timeNow, string block = "")
    {
        return ToolError.InvalidArguments("Wrong agrument");
    }
}