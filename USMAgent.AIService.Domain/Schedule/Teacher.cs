namespace USMAgent.Domain.Schedule;

public sealed class Teacher
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string ShortName { get; set; } = null!;

    public ICollection<ScheduleEntry> ScheduleEntries { get; set; } = new List<ScheduleEntry>();
}