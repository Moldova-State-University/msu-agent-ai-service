namespace USMAgent.Domain.Entities;
public class TimeSlot
{
    public int Id { get; set; }

    public int SlotNumber { get; set; }

    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public ICollection<ScheduleEntry> ScheduleEntries { get; set; }
        = new List<ScheduleEntry>();
}
