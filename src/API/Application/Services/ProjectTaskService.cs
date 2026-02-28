using Application.DTOs.ProjectTask;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Common;
using Domain.Entities;

namespace Application.Services;

public class ProjectTaskService : IProjectTaskService
{
    private readonly IProjectTaskRepository _taskRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IProjectRepository _projectRepository;

    public ProjectTaskService(
        IProjectTaskRepository taskRepository,
        IEmployeeRepository employeeRepository,
        IProjectRepository projectRepository)
    {
        _taskRepository = taskRepository;
        _employeeRepository = employeeRepository;
        _projectRepository = projectRepository;
    }

    public async Task<Result<ProjectTaskDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, true, cancellationToken);
        if (task == null)
            return Result<ProjectTaskDto>.Failure("Task not found");

        return Result<ProjectTaskDto>.Success(MapToDto(task));
    }

    public async Task<Result<IEnumerable<ProjectTaskDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tasks = await _taskRepository.GetAllAsync(true, cancellationToken);
        return Result<IEnumerable<ProjectTaskDto>>.Success(tasks.Select(MapToDto));
    }

    public async Task<Result<ProjectTaskDto>> CreateAsync(CreateProjectTaskDto createDto, CancellationToken cancellationToken = default)
    {
        // Validate dependencies
        if (await _employeeRepository.GetByIdAsync(createDto.AuthorId, true, cancellationToken) == null)
            return Result<ProjectTaskDto>.Failure("Author not found");
        
        if (await _employeeRepository.GetByIdAsync(createDto.ExecutorId, true, cancellationToken) == null)
            return Result<ProjectTaskDto>.Failure("Executor not found");

        if (await _projectRepository.GetByIdAsync(createDto.ProjectId, true, cancellationToken) == null)
            return Result<ProjectTaskDto>.Failure("Project not found");

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

        return Result<ProjectTaskDto>.Success(MapToDto(task));
    }

    public async Task<Result> UpdateAsync(UpdateProjectTaskDto updateDto, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(updateDto.Id, false, cancellationToken);
        if (task == null)
            return Result.Failure("Task not found");

        // Validate dependencies if changed
        if (task.AuthorId != updateDto.AuthorId && await _employeeRepository.GetByIdAsync(updateDto.AuthorId, true, cancellationToken) == null)
            return Result.Failure("Author not found");

        if (task.ExecutorId != updateDto.ExecutorId && await _employeeRepository.GetByIdAsync(updateDto.ExecutorId, true, cancellationToken) == null)
            return Result.Failure("Executor not found");

        if (task.ProjectId != updateDto.ProjectId && await _projectRepository.GetByIdAsync(updateDto.ProjectId, true, cancellationToken) == null)
            return Result.Failure("Project not found");

        task.Name = updateDto.Name;
        task.AuthorId = updateDto.AuthorId;
        task.ExecutorId = updateDto.ExecutorId;
        task.Status = updateDto.Status;
        task.Comment = updateDto.Comment;
        task.Priority = updateDto.Priority;
        task.ProjectId = updateDto.ProjectId;

        _taskRepository.Update(task);
        await _taskRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, false, cancellationToken);
        if (task == null)
            return Result.Failure("Task not found");

        _taskRepository.Delete(task);
        await _taskRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
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
