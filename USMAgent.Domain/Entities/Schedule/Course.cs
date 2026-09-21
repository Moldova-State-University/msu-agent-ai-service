namespace USMAgent.Domain.Entities.Schedule;
public class Course
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public ICollection<CourseAlias> Aliases { get; set; } = [];

    public ICollection<ScheduleEntry> ScheduleEntries { get; set; } = [];
}
