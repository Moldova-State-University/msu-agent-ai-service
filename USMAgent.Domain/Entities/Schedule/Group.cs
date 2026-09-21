namespace USMAgent.Domain.Entities.Schedule;
public class Group
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public string? Language { get; set; }

    public ICollection<ScheduleEntry> ScheduleEntries { get; set; } = [];
}