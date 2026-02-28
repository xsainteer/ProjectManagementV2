namespace Domain.Entities;

public class ProjectTask
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Task author (link to Employee)
    public int AuthorId { get; set; }
    public Employee Author { get; set; } = null!;

    // Task executor (link to Employee)
    public int ExecutorId { get; set; }
    public Employee Executor { get; set; } = null!;

    public ProjectTaskStatus Status { get; set; }
    public string? Comment { get; set; }
    public int Priority { get; set; }

    // Relationship to Project (One-to-Many)
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
}
