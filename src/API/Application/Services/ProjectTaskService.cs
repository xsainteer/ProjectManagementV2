using Application.DTOs.ProjectTask;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Common;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ProjectTaskService : IProjectTaskService
{
    private readonly IProjectTaskRepository _taskRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<ProjectTaskService> _logger;

    public ProjectTaskService(
        IProjectTaskRepository taskRepository,
        IEmployeeRepository employeeRepository,
        IProjectRepository projectRepository,
        ILogger<ProjectTaskService> logger)
    {
        _taskRepository = taskRepository;
        _employeeRepository = employeeRepository;
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async Task<Result<ProjectTaskDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var task = await _taskRepository.GetByIdAsync(id, true, cancellationToken);
            if (task == null)
                return Result<ProjectTaskDto>.Failure("Task not found", 404);

            return Result<ProjectTaskDto>.Success(MapToDto(task), 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting task with ID {TaskId}", id);
            return Result<ProjectTaskDto>.Failure("An internal error occurred", 500);
        }
    }

    public async Task<Result<IEnumerable<ProjectTaskDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var tasks = await _taskRepository.GetAllAsync(true, cancellationToken);
            return Result<IEnumerable<ProjectTaskDto>>.Success(tasks.Select(MapToDto), 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all tasks");
            return Result<IEnumerable<ProjectTaskDto>>.Failure("An internal error occurred", 500);
        }
    }

    public async Task<Result<ProjectTaskDto>> CreateAsync(CreateProjectTaskDto createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _employeeRepository.GetByIdAsync(createDto.AuthorId, true, cancellationToken) == null)
                return Result<ProjectTaskDto>.Failure("Author not found", 400);
            
            if (await _employeeRepository.GetByIdAsync(createDto.ExecutorId, true, cancellationToken) == null)
                return Result<ProjectTaskDto>.Failure("Executor not found", 400);

            if (await _projectRepository.GetByIdAsync(createDto.ProjectId, true, cancellationToken) == null)
                return Result<ProjectTaskDto>.Failure("Project not found", 400);

            var task = new ProjectTask
            {
                Name = createDto.Name,
                AuthorId = createDto.AuthorId,
                ExecutorId = createDto.ExecutorId,
                Status = createDto.Status,
                Comment = createDto.Comment,
                Priority = createDto.Priority,
                ProjectId = createDto.ProjectId
            };

            await _taskRepository.AddAsync(task, cancellationToken);
            await _taskRepository.SaveChangesAsync(cancellationToken);

            return Result<ProjectTaskDto>.Success(MapToDto(task), 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating task");
            return Result<ProjectTaskDto>.Failure("An internal error occurred", 500);
        }
    }

    public async Task<Result> UpdateAsync(UpdateProjectTaskDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var task = await _taskRepository.GetByIdAsync(updateDto.Id, false, cancellationToken);
            if (task == null)
                return Result.Failure("Task not found", 404);

            if (task.AuthorId != updateDto.AuthorId && await _employeeRepository.GetByIdAsync(updateDto.AuthorId, true, cancellationToken) == null)
                return Result.Failure("Author not found", 400);

            if (task.ExecutorId != updateDto.ExecutorId && await _employeeRepository.GetByIdAsync(updateDto.ExecutorId, true, cancellationToken) == null)
                return Result.Failure("Executor not found", 400);

            if (task.ProjectId != updateDto.ProjectId && await _projectRepository.GetByIdAsync(updateDto.ProjectId, true, cancellationToken) == null)
                return Result.Failure("Project not found", 400);

            task.Name = updateDto.Name;
            task.AuthorId = updateDto.AuthorId;
            task.ExecutorId = updateDto.ExecutorId;
            task.Status = updateDto.Status;
            task.Comment = updateDto.Comment;
            task.Priority = updateDto.Priority;
            task.ProjectId = updateDto.ProjectId;

            _taskRepository.Update(task);
            await _taskRepository.SaveChangesAsync(cancellationToken);

            return Result.Success(204);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating task with ID {TaskId}", updateDto.Id);
            return Result.Failure("An internal error occurred", 500);
        }
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var task = await _taskRepository.GetByIdAsync(id, false, cancellationToken);
            if (task == null)
                return Result.Failure("Task not found", 404);

            _taskRepository.Delete(task);
            await _taskRepository.SaveChangesAsync(cancellationToken);

            return Result.Success(204);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting task with ID {TaskId}", id);
            return Result.Failure("An internal error occurred", 500);
        }
    }

    private static ProjectTaskDto MapToDto(ProjectTask task) => new(
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
