using Application.DTOs.Project;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Common;
using Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<ProjectService> _logger;
    private readonly IValidator<CreateProjectDto> _createValidator;
    private readonly IValidator<CreateFullProjectDto> _createFullValidator;
    private readonly IValidator<UpdateProjectDto> _updateValidator;

    public ProjectService(
        IProjectRepository projectRepository, 
        IEmployeeRepository employeeRepository,
        ILogger<ProjectService> logger,
        IValidator<CreateProjectDto> createValidator,
        IValidator<CreateFullProjectDto> createFullValidator,
        IValidator<UpdateProjectDto> updateValidator)
    {
        _projectRepository = projectRepository;
        _employeeRepository = employeeRepository;
        _logger = logger;
        _createValidator = createValidator;
        _createFullValidator = createFullValidator;
        _updateValidator = updateValidator;
    }

    public async Task<Result<ProjectDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(id, true, cancellationToken);
            if (project == null)
                return Result<ProjectDto>.Failure(Error.NotFound($"Project with ID {id} was not found."));

            return Result<ProjectDto>.Success(MapToDto(project));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting project with ID {ProjectId}", id);
            return Result<ProjectDto>.Failure(Error.Unexpected("An internal error occurred"));
        }
    }

    public async Task<Result<IEnumerable<ProjectDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var projects = await _projectRepository.GetAllAsync(true, cancellationToken);
            return Result<IEnumerable<ProjectDto>>.Success(projects.Select(MapToDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all projects");
            return Result<IEnumerable<ProjectDto>>.Failure(Error.Unexpected("An internal error occurred"));
        }
    }

    public async Task<Result<(IEnumerable<ProjectDto> Items, int TotalCount)>> GetProjectsAsync(
        DateTime? startDateFrom,
        DateTime? startDateTo,
        int? priority,
        string? sortBy,
        bool sortDescending,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (items, totalCount) = await _projectRepository.GetProjectsAsync(
                startDateFrom, startDateTo, priority, sortBy, sortDescending, pageNumber, pageSize, true, cancellationToken);

            return Result<(IEnumerable<ProjectDto>, int)>.Success((items.Select(MapToDto), totalCount));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while filtering projects");
            return Result<(IEnumerable<ProjectDto>, int)>.Failure(Error.Unexpected("An internal error occurred"));
        }
    }

    public async Task<Result<ProjectDto>> CreateAsync(CreateProjectDto createDto, CancellationToken cancellationToken = default)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating project");
            return Result<ProjectDto>.Failure(Error.Unexpected("An internal error occurred"));
        }
    }

    public async Task<Result<ProjectDto>> CreateFullAsync(CreateFullProjectDto createDto, List<FileData> files, CancellationToken cancellationToken = default)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating full project");
            
            // lazy to do it another way, but it works for now
            if (ex.Message.Contains("not found"))
                return Result<ProjectDto>.Failure(Error.NotFound(ex.Message));
                
            return Result<ProjectDto>.Failure(Error.Unexpected("An internal error occurred during project creation."));
        }
    }

    public async Task<Result> UpdateAsync(UpdateProjectDto updateDto, CancellationToken cancellationToken = default)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating project with ID {ProjectId}", updateDto.Id);
            return Result.Failure(Error.Unexpected("An internal error occurred"));
        }
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(id, false, cancellationToken);
            if (project == null)
                return Result.Failure(Error.NotFound($"Project with ID {id} was not found."));

            _projectRepository.Delete(project);
            await _projectRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting project with ID {ProjectId}", id);
            return Result.Failure(Error.Unexpected("An internal error occurred"));
        }
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
