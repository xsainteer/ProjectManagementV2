using Application.DTOs.Project;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Common;
using Domain.Entities;

namespace Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public ProjectService(IProjectRepository projectRepository, IEmployeeRepository employeeRepository)
    {
        _projectRepository = projectRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<Result<ProjectDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, true, cancellationToken);
        if (project == null)
            return Result<ProjectDto>.Failure("Project not found");

        return Result<ProjectDto>.Success(MapToDto(project));
    }

    public async Task<Result<IEnumerable<ProjectDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.GetAllAsync(true, cancellationToken);
        return Result<IEnumerable<ProjectDto>>.Success(projects.Select(MapToDto));
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
        var (items, totalCount) = await _projectRepository.GetProjectsAsync(
            startDateFrom, startDateTo, priority, sortBy, sortDescending, pageNumber, pageSize, true, cancellationToken);

        return Result<(IEnumerable<ProjectDto>, int)>.Success((items.Select(MapToDto), totalCount));
    }

    public async Task<Result<ProjectDto>> CreateAsync(CreateProjectDto createDto, CancellationToken cancellationToken = default)
    {
        // Validate manager exists
        var manager = await _employeeRepository.GetByIdAsync(createDto.ProjectManagerId, true, cancellationToken);
        if (manager == null)
            return Result<ProjectDto>.Failure("Project manager not found");

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

    public async Task<Result> UpdateAsync(UpdateProjectDto updateDto, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(updateDto.Id, false, cancellationToken);
        if (project == null)
            return Result.Failure("Project not found");

        // Validate manager exists if changed
        if (project.ProjectManagerId != updateDto.ProjectManagerId)
        {
            var manager = await _employeeRepository.GetByIdAsync(updateDto.ProjectManagerId, true, cancellationToken);
            if (manager == null)
                return Result.Failure("Project manager not found");
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
            return Result.Failure("Project not found");

        _projectRepository.Delete(project);
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
