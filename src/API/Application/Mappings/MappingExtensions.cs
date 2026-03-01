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

    public static ProjectDocumentDto ToDto(this ProjectDocument document) => new(
        document.Id,
        document.FileName,
        document.FilePath,
        document.ProjectId
    );

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
}
