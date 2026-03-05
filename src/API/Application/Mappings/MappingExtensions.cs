using Application.DTOs.Employee;
using Application.DTOs.Project;
using Application.DTOs.ProjectDocument;
using Application.DTOs.ProjectTask;
using Domain.Entities;

namespace Application.Mappings;

public static class MappingExtensions
{
    public static EmployeeDto ToDto(this Employee employee) => new(
        employee.Id,
        employee.FirstName,
        employee.LastName,
        employee.MiddleName,
        employee.Email
    );

    public static Employee ToEntity(this CreateEmployeeDto dto) => new()
    {
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        MiddleName = dto.MiddleName,
        Email = dto.Email
    };

    public static void UpdateWith(this Employee employee, UpdateEmployeeDto dto)
    {
        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.MiddleName = dto.MiddleName;
        employee.Email = dto.Email;
    }

    public static ProjectDocumentDto ToDto(this ProjectDocument document) => new(
        document.Id,
        document.FileName,
        document.FilePath,
        document.ProjectId
    );

    public static ProjectDocument ToEntity(this CreateProjectDocumentDto dto, string? fileName = null, string? filePath = null) => new()
    {
        FileName = fileName ?? dto.FileName,
        FilePath = filePath ?? string.Empty,
        ProjectId = dto.ProjectId
    };

    public static void UpdateWith(this ProjectDocument document, ProjectDocumentDto dto)
    {
        document.FileName = dto.FileName;
        document.FilePath = dto.FilePath;
        document.ProjectId = dto.ProjectId;
    }

    public static ProjectDto ToDto(this Project project) => new(
        project.Id,
        project.Name,
        project.CustomerCompany,
        project.PerformerCompany,
        project.ProjectManagerId,
        project.StartDate,
        project.EndDate,
        project.Priority
    );

    public static ProjectDetailsDto ToDetailsDto(this Project project) => new(
        project.Id,
        project.Name,
        project.CustomerCompany,
        project.PerformerCompany,
        project.ProjectManager.ToDto(),
        project.StartDate,
        project.EndDate,
        project.Priority,
        project.Employees.Select(e => e.ToDto()),
        project.Documents.Select(d => d.ToDto())
    );

    public static Project ToEntity(this CreateProjectDto dto) => new()
    {
        Name = dto.Name,
        CustomerCompany = dto.CustomerCompany,
        PerformerCompany = dto.PerformerCompany,
        ProjectManagerId = dto.ProjectManagerId,
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        Priority = dto.Priority
    };

    public static Project ToEntity(this CreateFullProjectDto dto) => new()
    {
        Name = dto.Name,
        CustomerCompany = dto.CustomerCompany,
        PerformerCompany = dto.PerformerCompany,
        ProjectManagerId = dto.ProjectManagerId,
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        Priority = dto.Priority
    };

    public static void UpdateWith(this Project project, UpdateProjectDto dto)
    {
        project.Name = dto.Name;
        project.CustomerCompany = dto.CustomerCompany;
        project.PerformerCompany = dto.PerformerCompany;
        project.ProjectManagerId = dto.ProjectManagerId;
        project.StartDate = dto.StartDate;
        project.EndDate = dto.EndDate;
        project.Priority = dto.Priority;
    }

    public static ProjectTaskDto ToDto(this ProjectTask task) => new(
        task.Id,
        task.Name,
        task.AuthorId,
        task.ExecutorId,
        task.Status,
        task.Comment,
        task.Priority,
        task.ProjectId
    );

    public static ProjectTask ToEntity(this CreateProjectTaskDto dto) => new()
    {
        Name = dto.Name,
        AuthorId = dto.AuthorId,
        ExecutorId = dto.ExecutorId,
        Status = dto.Status,
        Comment = dto.Comment,
        Priority = dto.Priority,
        ProjectId = dto.ProjectId
    };

    public static void UpdateWith(this ProjectTask task, UpdateProjectTaskDto dto)
    {
        task.Name = dto.Name;
        task.AuthorId = dto.AuthorId;
        task.ExecutorId = dto.ExecutorId;
        task.Status = dto.Status;
        task.Comment = dto.Comment;
        task.Priority = dto.Priority;
    }
}
