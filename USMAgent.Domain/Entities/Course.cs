namespace USMAgent.Domain.Entities;
public class Course
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public ICollection<CourseAlias> Aliases { get; set; }
        = new List<CourseAlias>();

    public ICollection<ScheduleEntry> ScheduleEntries { get; set; }
        = new List<ScheduleEntry>();
}
