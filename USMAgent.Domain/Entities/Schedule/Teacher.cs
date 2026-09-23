namespace USMAgent.Domain.Entities.Schedule;
public class Teacher
{
    public int Id { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ShortName { get; set; }

    public ICollection<ScheduleEntry> ScheduleEntries { get; set; } = [];
}
