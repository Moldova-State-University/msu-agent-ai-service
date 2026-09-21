namespace USMAgent.Domain.Entities;
public class Group
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public string? Language { get; set; }

    public ICollection<ScheduleEntryGroup> ScheduleEntryGroups { get; set; }
        = new List<ScheduleEntryGroup>();
}