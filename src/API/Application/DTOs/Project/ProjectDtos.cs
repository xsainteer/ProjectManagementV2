namespace Application.DTOs.Project;

public record ProjectDto(
    int Id,
    string Name,
    string CustomerCompany,
    string PerformerCompany,
    int ProjectManagerId,
    DateTime StartDate,
    DateTime? EndDate,
    int Priority
);

public record CreateProjectDto(
    string Name,
    string CustomerCompany,
    string PerformerCompany,
    int ProjectManagerId,
    DateTime StartDate,
    DateTime? EndDate,
    int Priority
);

public record UpdateProjectDto(
    int Id,
    string Name,
    string CustomerCompany,
    string PerformerCompany,
    int ProjectManagerId,
    DateTime StartDate,
    DateTime? EndDate,
    int Priority
);
