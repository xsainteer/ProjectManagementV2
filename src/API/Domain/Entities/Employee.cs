using Domain.Common;

namespace Domain.Entities;

public class Employee : IEntity
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Projects where the employee is a regular participant (Many-to-Many)
    public ICollection<Project> Projects { get; set; } = new List<Project>();

    // Projects managed by the employee (One-to-Many as Project Manager)
    public ICollection<Project> ManagedProjects { get; set; } = new List<Project>();

    // Tasks authored by the employee (One-to-Many)
    public ICollection<ProjectTask> AuthoredTasks { get; set; } = new List<ProjectTask>();

    // Tasks assigned to the employee (One-to-Many)
    public ICollection<ProjectTask> ExecutedTasks { get; set; } = new List<ProjectTask>();
}
