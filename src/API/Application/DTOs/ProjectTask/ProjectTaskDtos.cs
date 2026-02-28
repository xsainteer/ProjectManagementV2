using Domain.Entities;

namespace Application.DTOs.ProjectTask;

public record ProjectTaskDto(
    int Id,
    string Name,
    int AuthorId,
    int ExecutorId,
    ProjectTaskStatus Status,
    string? Comment,
    int Priority,
    int ProjectId
);

public record CreateProjectTaskDto(
    string Name,
    int AuthorId,
    int ExecutorId,
    ProjectTaskStatus Status,
    string? Comment,
    int Priority,
    int ProjectId
);

public record UpdateProjectTaskDto(
    int Id,
    string Name,
    int AuthorId,
    int ExecutorId,
    ProjectTaskStatus Status,
    string? Comment,
    int Priority,
    int ProjectId
);
