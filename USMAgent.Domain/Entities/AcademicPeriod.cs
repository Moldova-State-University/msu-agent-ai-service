namespace USMAgent.Domain.Entities;

public class AcademicPeriod
{
    public int Id { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public ICollection<ScheduleEntry> ScheduleEntries { get; set; }
        = new List<ScheduleEntry>();
}
