using USMAgent.Domain.Enums;
using DayOfWeek = USMAgent.Domain.Enums.DayOfWeek;

namespace USMAgent.Domain.Entities.Schedule;
public class ScheduleEntry
{
    public int Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }

    public Parity? Parity { get; set; }

    public LessonType? LessonType { get; set; }

    public string? Subgroup { get; set; }

    public string? Specialization { get; set; }

    public string? Alternative { get; set; }

    public AcademicPeriod AcademicPeriod { get; set; } = null!;

    public TimeSlot TimeSlot { get; set; } = null!;

    public Course Course { get; set; } = null!;

    public Room? Room { get; set; }

    public ICollection<Teacher> Teachers { get; set; } = [];

    public ICollection<Group> Groups { get; set; } = [];
}
