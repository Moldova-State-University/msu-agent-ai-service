namespace USMAgent.Domain.Entities;
public class ScheduleEntryGroup
{
    public int ScheduleEntryId { get; set; }

    public int GroupId { get; set; }

    public ScheduleEntry ScheduleEntry { get; set; } = null!;

    public Group Group { get; set; } = null!;
}