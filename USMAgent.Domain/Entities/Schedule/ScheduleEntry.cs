namespace USMAgent.Domain.Entities.Schedule;
public class ScheduleEntry
{
    public int Id { get; set; }

    public int AcademicPeriodId { get; set; }

    public int DayOfWeek { get; set; }

    public string? Parity { get; set; }

    public int TimeSlotId { get; set; }

    public int CourseId { get; set; }

    public int? RoomId { get; set; }

    public string? LessonType { get; set; }

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
