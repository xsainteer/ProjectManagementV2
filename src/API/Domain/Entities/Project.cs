namespace Domain.Entities;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CustomerCompany { get; set; } = string.Empty;
    public string PerformerCompany { get; set; } = string.Empty;

    // Project Manager (one of the employees)
    public int ProjectManagerId { get; set; }
    public Employee ProjectManager { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Priority { get; set; }

    // Employees assigned to the project (Many-to-Many)
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();

    // Tasks belonging to the project (One-to-Many)
    public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
    
    // Documents related to the project (One-to-Many)
    public ICollection<ProjectDocument> Documents { get; set; } = new List<ProjectDocument>();
}
