using Application.DTOs.ProjectTask;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Common;
using Domain.Entities;
using FluentValidation;

namespace Application.Services;

public class ProjectTaskService : IProjectTaskService
{
    private readonly IProjectTaskRepository _taskRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IValidator<CreateProjectTaskDto> _createValidator;
    private readonly IValidator<UpdateProjectTaskDto> _updateValidator;

    public ProjectTaskService(
        IProjectTaskRepository taskRepository,
        IEmployeeRepository employeeRepository,
        IProjectRepository projectRepository,
        IValidator<CreateProjectTaskDto> createValidator,
        IValidator<UpdateProjectTaskDto> updateValidator)
    {
        _taskRepository = taskRepository;
        _employeeRepository = employeeRepository;
        _projectRepository = projectRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<Result<ProjectTaskDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, true, cancellationToken);
        if (task == null)
            return Result<ProjectTaskDto>.Failure(Error.NotFound($"Task with ID {id} was not found."));

        return Result<ProjectTaskDto>.Success(MapToDto(task));
    }

    public async Task<Result<IEnumerable<ProjectTaskDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tasks = await _taskRepository.GetAllAsync(true, cancellationToken);
        return Result<IEnumerable<ProjectTaskDto>>.Success(tasks.Select(MapToDto));
    }

    public async Task<Result<ProjectTaskDto>> CreateAsync(CreateProjectTaskDto createDto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(createDto, cancellationToken);
        if (!validationResult.IsValid)
            return Result<ProjectTaskDto>.Failure(Error.Validation(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));

        if (await _employeeRepository.GetByIdAsync(createDto.AuthorId, true, cancellationToken) == null)
            return Result<ProjectTaskDto>.Failure(Error.NotFound($"Author with ID {createDto.AuthorId} was not found."));
        
        if (await _employeeRepository.GetByIdAsync(createDto.ExecutorId, true, cancellationToken) == null)
            return Result<ProjectTaskDto>.Failure(Error.NotFound($"Executor with ID {createDto.ExecutorId} was not found."));

        if (await _projectRepository.GetByIdAsync(createDto.ProjectId, true, cancellationToken) == null)
            return Result<ProjectTaskDto>.Failure(Error.NotFound($"Project with ID {createDto.ProjectId} was not found."));

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
        var validationResult = await _updateValidator.ValidateAsync(updateDto, cancellationToken);
        if (!validationResult.IsValid)
            return Result.Failure(Error.Validation(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));

        var task = await _taskRepository.GetByIdAsync(updateDto.Id, false, cancellationToken);
        if (task == null)
            return Result.Failure(Error.NotFound($"Task with ID {updateDto.Id} was not found."));

        if (task.AuthorId != updateDto.AuthorId && await _employeeRepository.GetByIdAsync(updateDto.AuthorId, true, cancellationToken) == null)
            return Result.Failure(Error.NotFound($"Author with ID {updateDto.AuthorId} was not found."));

        if (task.ExecutorId != updateDto.ExecutorId && await _employeeRepository.GetByIdAsync(updateDto.ExecutorId, true, cancellationToken) == null)
            return Result.Failure(Error.NotFound($"Executor with ID {updateDto.ExecutorId} was not found."));

        if (task.ProjectId != updateDto.ProjectId && await _projectRepository.GetByIdAsync(updateDto.ProjectId, true, cancellationToken) == null)
            return Result.Failure(Error.NotFound($"Project with ID {updateDto.ProjectId} was not found."));

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
            return Result.Failure(Error.NotFound($"Task with ID {id} was not found."));

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
