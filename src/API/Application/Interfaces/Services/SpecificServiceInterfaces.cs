using Application.DTOs.Employee;
using Application.DTOs.Project;
using Application.DTOs.ProjectDocument;
using Application.DTOs.ProjectTask;
using Domain.Common;
using Domain.Entities;

namespace Application.Interfaces.Services;

public interface IEmployeeService : IService<Employee, EmployeeDto, CreateEmployeeDto, UpdateEmployeeDto>
{
    Task<Result<IEnumerable<EmployeeDto>>> GetAllAsync(string? searchTerm, CancellationToken cancellationToken = default);
}

public interface IProjectService : IService<Project, ProjectDto, CreateProjectDto, UpdateProjectDto>
{
    Task<Result<PaginatedResultDto<ProjectDto>>> GetProjectsAsync(
        DateTime? startDateFrom,
        DateTime? startDateTo,
        int? priority,
        string? sortBy,
        bool sortDescending,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Result<ProjectDto>> CreateFullAsync(CreateFullProjectDto createDto, List<FileData> files, CancellationToken cancellationToken = default);

    Task<Result> AddEmployeeAsync(int projectId, int employeeId, CancellationToken cancellationToken = default);
    Task<Result> RemoveEmployeeAsync(int projectId, int employeeId, CancellationToken cancellationToken = default);
}

public interface IProjectTaskService : IService<ProjectTask, ProjectTaskDto, CreateProjectTaskDto, UpdateProjectTaskDto>
{
}

public interface IProjectDocumentService : IService<ProjectDocument, ProjectDocumentDto, CreateProjectDocumentDto, ProjectDocumentDto>
{
    Task<Result<ProjectDocumentDto>> CreateWithFileAsync(CreateProjectDocumentDto createDto, Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<Result> UpdateWithFileAsync(ProjectDocumentDto updateDto, Stream? fileStream, string? fileName, CancellationToken cancellationToken = default);
    Task<Result<Stream>> GetFileStreamAsync(int id, CancellationToken cancellationToken = default);
}
