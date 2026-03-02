using Application.DTOs.ProjectTask;
using Application.Interfaces;
using Application.Interfaces.Services;
using Application.Mappings;
using Application.Validators;
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

        return Result<ProjectTaskDto>.Success(task.ToDto());
    }

    public async Task<Result<IEnumerable<ProjectTaskDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tasks = await _taskRepository.GetAllAsync(true, cancellationToken);
        return Result<IEnumerable<ProjectTaskDto>>.Success(tasks.Select(t => t.ToDto()));
    }

    public async Task<Result<ProjectTaskDto>> CreateAsync(CreateProjectTaskDto createDto, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateToResultAsync(createDto, cancellationToken);
        if (validation.IsFailure) return Result<ProjectTaskDto>.Failure(validation.Error);

        var project = await _projectRepository.GetWithEmployeesAsync(createDto.ProjectId, cancellationToken);
        if (project == null)
            return Result<ProjectTaskDto>.Failure(Error.NotFound($"Project with ID {createDto.ProjectId} was not found."));

        if (!IsEmployeeAssignedToProject(project, createDto.AuthorId))
            return Result<ProjectTaskDto>.Failure(Error.Validation($"Author with ID {createDto.AuthorId} is not assigned to this project."));

        if (!IsEmployeeAssignedToProject(project, createDto.ExecutorId))
            return Result<ProjectTaskDto>.Failure(Error.Validation($"Executor with ID {createDto.ExecutorId} is not assigned to this project."));

        var task = createDto.ToEntity();

        await _taskRepository.AddAsync(task, cancellationToken);
        await _taskRepository.SaveChangesAsync(cancellationToken);

        return Result<ProjectTaskDto>.Success(task.ToDto());
    }

    public async Task<Result> UpdateAsync(UpdateProjectTaskDto updateDto, CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateToResultAsync(updateDto, cancellationToken);
        if (validation.IsFailure) return validation;

        var task = await _taskRepository.GetByIdAsync(updateDto.Id, false, cancellationToken);
        if (task == null)
            return Result.Failure(Error.NotFound($"Task with ID {updateDto.Id} was not found."));

        var project = await _projectRepository.GetWithEmployeesAsync(task.ProjectId, cancellationToken);
        if (project == null)
            return Result.Failure(Error.NotFound($"Project with ID {task.ProjectId} was not found."));

        if (!IsEmployeeAssignedToProject(project, updateDto.AuthorId))
            return Result.Failure(Error.Validation($"Author with ID {updateDto.AuthorId} is not assigned to this project."));

        if (!IsEmployeeAssignedToProject(project, updateDto.ExecutorId))
            return Result.Failure(Error.Validation($"Executor with ID {updateDto.ExecutorId} is not assigned to this project."));

        task.UpdateWith(updateDto);

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

    private static bool IsEmployeeAssignedToProject(Project project, int employeeId)
    {
        return project.ProjectManagerId == employeeId || project.Employees.Any(e => e.Id == employeeId);
    }
}
