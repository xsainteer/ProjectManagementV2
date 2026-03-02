using Application.DTOs.Project;
using Application.Interfaces;
using Application.Interfaces.Services;
using Application.Mappings;
using Application.Validators;
using Domain.Common;
using Domain.Entities;
using FluentValidation;

namespace Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IFileService _fileService;
    private readonly IValidator<CreateProjectDto> _createValidator;
    private readonly IValidator<CreateFullProjectDto> _createFullValidator;
    private readonly IValidator<UpdateProjectDto> _updateValidator;

    public ProjectService(
        IProjectRepository projectRepository, 
        IEmployeeRepository employeeRepository,
        IFileService fileService,
        IValidator<CreateProjectDto> createValidator,
        IValidator<CreateFullProjectDto> createFullValidator,
        IValidator<UpdateProjectDto> updateValidator)
    {
        _projectRepository = projectRepository;
        _employeeRepository = employeeRepository;
        _fileService = fileService;
        _createValidator = createValidator;
        _createFullValidator = createFullValidator;
        _updateValidator = updateValidator;
    }

    public async Task<Result<ProjectDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, true, cancellationToken);
        if (project == null)
            return Result<ProjectDto>.Failure(Error.NotFound($"Project with ID {id} was not found."));

        return Result<ProjectDto>.Success(project.ToDto());
    }

    public async Task<Result<IEnumerable<ProjectDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.GetAllAsync(true, cancellationToken);
        return Result<IEnumerable<ProjectDto>>.Success(projects.Select(p => p.ToDto()));
    }

    public async Task<Result<PaginatedResultDto<ProjectDto>>> GetProjectsAsync(
        GetProjectsRequestDto requestDto,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _projectRepository.GetProjectsAsync(requestDto, true, cancellationToken);

        return Result<PaginatedResultDto<ProjectDto>>.Success(new PaginatedResultDto<ProjectDto>(items.Select(p => p.ToDto()), totalCount));
    }

    public async Task<Result<ProjectDto>> CreateAsync(CreateProjectDto createDto, CancellationToken cancellationToken = default)
    {
        var validation = await _createValidator.ValidateToResultAsync(createDto, cancellationToken);
        if (validation.IsFailure) return Result<ProjectDto>.Failure(validation.Error);

        var manager = await _employeeRepository.GetByIdAsync(createDto.ProjectManagerId, true, cancellationToken);
        if (manager == null)
            return Result<ProjectDto>.Failure(Error.NotFound($"Project manager with ID {createDto.ProjectManagerId} was not found."));

        var project = createDto.ToEntity();

        await _projectRepository.AddAsync(project, cancellationToken);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        return Result<ProjectDto>.Success(project.ToDto());
    }

    public async Task<Result<ProjectDto>> CreateFullAsync(CreateFullProjectDto createDto, List<FileData> files, CancellationToken cancellationToken = default)
    {
        var validation = await _createFullValidator.ValidateToResultAsync(createDto, cancellationToken);
        if (validation.IsFailure) return Result<ProjectDto>.Failure(validation.Error);

        var manager = await _employeeRepository.GetByIdAsync(createDto.ProjectManagerId, true, cancellationToken);
        if (manager == null)
            return Result<ProjectDto>.Failure(Error.NotFound($"Project manager with ID {createDto.ProjectManagerId} was not found."));

        await using var transaction = await _projectRepository.BeginTransactionAsync(cancellationToken);
        var savedFilePaths = new List<string>();

        try
        {
            var project = createDto.ToEntity();

            foreach (var executorId in createDto.ExecutorIds)
            {
                var executor = await _employeeRepository.GetByIdAsync(executorId, false, cancellationToken);
                if (executor == null)
                    return Result<ProjectDto>.Failure(Error.NotFound($"Executor with ID {executorId} was not found."));
                
                project.Employees.Add(executor);
            }

            await _projectRepository.AddAsync(project, cancellationToken);
            await _projectRepository.SaveChangesAsync(cancellationToken);

            foreach (var file in files)
            {
                var saveResult = await _fileService.SaveFileAsync(file.Stream, file.FileName, $"project_{project.Id}", cancellationToken);
                if (saveResult.IsFailure) throw new Exception(saveResult.Error.Message);
                
                savedFilePaths.Add(saveResult.Value);
                
                project.Documents.Add(new ProjectDocument
                {
                    FileName = file.FileName,
                    FilePath = saveResult.Value,
                    ProjectId = project.Id
                });
            }

            await _projectRepository.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result<ProjectDto>.Success(project.ToDto());
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            foreach (var path in savedFilePaths) _fileService.DeleteFile(path);
            throw;
        }
    }

    public async Task<Result> UpdateAsync(UpdateProjectDto updateDto, CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateToResultAsync(updateDto, cancellationToken);
        if (validation.IsFailure) return validation;

        var project = await _projectRepository.GetByIdAsync(updateDto.Id, false, cancellationToken);
        if (project == null)
            return Result.Failure(Error.NotFound($"Project with ID {updateDto.Id} was not found."));

        if (project.ProjectManagerId != updateDto.ProjectManagerId)
        {
            var manager = await _employeeRepository.GetByIdAsync(updateDto.ProjectManagerId, true, cancellationToken);
            if (manager == null)
                return Result.Failure(Error.NotFound($"Project manager with ID {updateDto.ProjectManagerId} was not found."));
        }

        project.UpdateWith(updateDto);

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
}
