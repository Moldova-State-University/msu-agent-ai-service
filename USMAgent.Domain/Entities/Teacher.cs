namespace USMAgent.Domain.Entities;
public class Teacher
{
    public int Id { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ShortName { get; set; }

    public ICollection<ScheduleEntryTeacher> ScheduleEntryTeachers { get; set; }
        = new List<ScheduleEntryTeacher>();
}
