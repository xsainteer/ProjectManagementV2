using Domain.Common;

namespace Domain.Entities;

public class ProjectDocument : IEntity
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
}