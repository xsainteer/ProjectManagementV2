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

public record CreateFullProjectDto(
    string Name,
    string CustomerCompany,
    string PerformerCompany,
    int ProjectManagerId,
    DateTime StartDate,
    DateTime? EndDate,
    int Priority,
    List<int> ExecutorIds
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

public record FileData(Stream Stream, string FileName);

public record PaginatedResultDto<T>(IEnumerable<T> Items, int TotalCount);
