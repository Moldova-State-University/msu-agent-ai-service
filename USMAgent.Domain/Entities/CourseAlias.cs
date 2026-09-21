namespace USMAgent.Domain.Entities;
public class CourseAlias
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public string Alias { get; set; } = null!;

    public Course Course { get; set; } = null!;
}