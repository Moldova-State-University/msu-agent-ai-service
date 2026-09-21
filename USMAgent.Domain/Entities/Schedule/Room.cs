namespace USMAgent.Domain.Entities.Schedule;
public class Room
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public ICollection<ScheduleEntry> ScheduleEntries { get; set; } = [];
}
