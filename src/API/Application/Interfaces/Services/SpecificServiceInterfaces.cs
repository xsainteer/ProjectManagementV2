using Application.DTOs.Employee;
using Application.DTOs.Project;
using Application.DTOs.ProjectDocument;
using Application.DTOs.ProjectTask;
using Domain.Common;
using Domain.Entities;

namespace Application.Interfaces.Services;

public interface IEmployeeService : IService<Employee, EmployeeDto, CreateEmployeeDto, UpdateEmployeeDto>
{
}

public interface IProjectService : IService<Project, ProjectDto, CreateProjectDto, UpdateProjectDto>
{
    Task<Result<(IEnumerable<ProjectDto> Items, int TotalCount)>> GetProjectsAsync(
        DateTime? startDateFrom,
        DateTime? startDateTo,
        int? priority,
        string? sortBy,
        bool sortDescending,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}

public interface IProjectTaskService : IService<ProjectTask, ProjectTaskDto, CreateProjectTaskDto, UpdateProjectTaskDto>
{
}

public interface IProjectDocumentService : IService<ProjectDocument, ProjectDocumentDto, CreateProjectDocumentDto, ProjectDocumentDto>
{
}
