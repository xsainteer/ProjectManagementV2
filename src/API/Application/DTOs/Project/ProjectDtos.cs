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

public record ProjectDetailsDto(
    int Id,
    string Name,
    string CustomerCompany,
    string PerformerCompany,
    Application.DTOs.Employee.EmployeeDto ProjectManager,
    DateTime StartDate,
    DateTime? EndDate,
    int Priority,
    IEnumerable<Application.DTOs.Employee.EmployeeDto> Employees,
    IEnumerable<Application.DTOs.ProjectDocument.ProjectDocumentDto> Documents
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

public record GetProjectsRequestDto(
    DateTime? StartDateFrom = null,
    DateTime? StartDateTo = null,
    int? Priority = null,
    string? SortBy = null,
    bool SortDescending = false,
    int PageNumber = 1,
    int PageSize = 10
);

public record FileData(Stream Stream, string FileName);

public record PaginatedResultDto<T>(IEnumerable<T> Items, int TotalCount);
