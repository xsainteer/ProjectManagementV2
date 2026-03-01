using Application.DTOs.Project;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Common;
using Domain.Entities;
using FluentValidation;

namespace Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IValidator<CreateProjectDto> _createValidator;
    private readonly IValidator<CreateFullProjectDto> _createFullValidator;
    private readonly IValidator<UpdateProjectDto> _updateValidator;

    public ProjectService(
        IProjectRepository projectRepository, 
        IEmployeeRepository employeeRepository,
        IValidator<CreateProjectDto> createValidator,
        IValidator<CreateFullProjectDto> createFullValidator,
        IValidator<UpdateProjectDto> updateValidator)
    {
        _projectRepository = projectRepository;
        _employeeRepository = employeeRepository;
        _createValidator = createValidator;
        _createFullValidator = createFullValidator;
        _updateValidator = updateValidator;
    }

    public async Task<Result<ProjectDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, true, cancellationToken);
        if (project == null)
            return Result<ProjectDto>.Failure(Error.NotFound($"Project with ID {id} was not found."));

        return Result<ProjectDto>.Success(MapToDto(project));
    }

    public async Task<Result<IEnumerable<ProjectDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.GetAllAsync(true, cancellationToken);
        return Result<IEnumerable<ProjectDto>>.Success(projects.Select(MapToDto));
    }

    public async Task<Result<PaginatedResultDto<ProjectDto>>> GetProjectsAsync(
        DateTime? startDateFrom,
        DateTime? startDateTo,
        int? priority,
        string? sortBy,
        bool sortDescending,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _projectRepository.GetProjectsAsync(
            startDateFrom, startDateTo, priority, sortBy, sortDescending, pageNumber, pageSize, true, cancellationToken);

        return Result<PaginatedResultDto<ProjectDto>>.Success(new PaginatedResultDto<ProjectDto>(items.Select(MapToDto), totalCount));
    }

    public async Task<Result<ProjectDto>> CreateAsync(CreateProjectDto createDto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(createDto, cancellationToken);
        if (!validationResult.IsValid)
            return Result<ProjectDto>.Failure(Error.Validation(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));

        var manager = await _employeeRepository.GetByIdAsync(createDto.ProjectManagerId, true, cancellationToken);
        if (manager == null)
            return Result<ProjectDto>.Failure(Error.NotFound($"Project manager with ID {createDto.ProjectManagerId} was not found."));

        var project = new Project
        {
            Name = createDto.Name,
            CustomerCompany = createDto.CustomerCompany,
            PerformerCompany = createDto.PerformerCompany,
            ProjectManagerId = createDto.ProjectManagerId,
            StartDate = createDto.StartDate,
            EndDate = createDto.EndDate,
            Priority = createDto.Priority
        };

        await _projectRepository.AddAsync(project, cancellationToken);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        return Result<ProjectDto>.Success(MapToDto(project));
    }

    public async Task<Result<ProjectDto>> CreateFullAsync(CreateFullProjectDto createDto, List<FileData> files, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createFullValidator.ValidateAsync(createDto, cancellationToken);
        if (!validationResult.IsValid)
            return Result<ProjectDto>.Failure(Error.Validation(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));

        var project = new Project
        {
            Name = createDto.Name,
            CustomerCompany = createDto.CustomerCompany,
            PerformerCompany = createDto.PerformerCompany,
            ProjectManagerId = createDto.ProjectManagerId,
            StartDate = createDto.StartDate,
            EndDate = createDto.EndDate,
            Priority = createDto.Priority
        };

        var createdProject = await _projectRepository.CreateFullAsync(project, createDto.ExecutorIds, files, cancellationToken);

        return Result<ProjectDto>.Success(MapToDto(createdProject));
    }

    public async Task<Result> UpdateAsync(UpdateProjectDto updateDto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(updateDto, cancellationToken);
        if (!validationResult.IsValid)
            return Result.Failure(Error.Validation(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));

        var project = await _projectRepository.GetByIdAsync(updateDto.Id, false, cancellationToken);
        if (project == null)
            return Result.Failure(Error.NotFound($"Project with ID {updateDto.Id} was not found."));

        if (project.ProjectManagerId != updateDto.ProjectManagerId)
        {
            var manager = await _employeeRepository.GetByIdAsync(updateDto.ProjectManagerId, true, cancellationToken);
            if (manager == null)
                return Result.Failure(Error.NotFound($"Project manager with ID {updateDto.ProjectManagerId} was not found."));
        }

        project.Name = updateDto.Name;
        project.CustomerCompany = updateDto.CustomerCompany;
        project.PerformerCompany = updateDto.PerformerCompany;
        project.ProjectManagerId = updateDto.ProjectManagerId;
        project.StartDate = updateDto.StartDate;
        project.EndDate = updateDto.EndDate;
        project.Priority = updateDto.Priority;

        _projectRepository.Update(project);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, false, cancellationToken);
        if (project == null)
            return Result.Failure(Error.NotFound($"Project with ID {id} was not found."));

        _projectRepository.Delete(project);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> AddEmployeeAsync(int projectId, int employeeId, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetWithEmployeesAsync(projectId, cancellationToken);
        if (project == null)
            return Result.Failure(Error.NotFound($"Project with ID {projectId} was not found."));

        if (project.Employees.Any(e => e.Id == employeeId))
            return Result.Failure(Error.Conflict($"Employee with ID {employeeId} is already assigned to this project."));

        var employee = await _employeeRepository.GetByIdAsync(employeeId, false, cancellationToken);
        if (employee == null)
            return Result.Failure(Error.NotFound($"Employee with ID {employeeId} was not found."));

        project.Employees.Add(employee);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RemoveEmployeeAsync(int projectId, int employeeId, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetWithEmployeesAsync(projectId, cancellationToken);
        if (project == null)
            return Result.Failure(Error.NotFound($"Project with ID {projectId} was not found."));

        var employee = project.Employees.FirstOrDefault(e => e.Id == employeeId);
        if (employee == null)
            return Result.Failure(Error.NotFound($"Employee with ID {employeeId} is not assigned to this project."));

        project.Employees.Remove(employee);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static ProjectDto MapToDto(Project project) => new(
        project.Id,
        project.Name,
        project.CustomerCompany,
        project.PerformerCompany,
        project.ProjectManagerId,
        project.StartDate,
        project.EndDate,
        project.Priority
    );
}
