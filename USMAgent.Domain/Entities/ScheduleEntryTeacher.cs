namespace USMAgent.Domain.Entities;
public class ScheduleEntryTeacher
{
    public int ScheduleEntryId { get; set; }

    public int TeacherId { get; set; }

    public ScheduleEntry ScheduleEntry { get; set; } = null!;

    public Teacher Teacher { get; set; } = null!;
}