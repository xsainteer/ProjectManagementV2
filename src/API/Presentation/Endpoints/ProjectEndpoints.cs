using Application.DTOs.Project;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Endpoints;

public static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects").WithTags("Projects");

        group.MapGet("/", GetProjects);
        group.MapGet("/{id:int}", GetProjectById);
        group.MapGet("/{id:int}/details", GetProjectDetails);
        group.MapPost("/", CreateProject);
        group.MapPost("/full", CreateFullProject).DisableAntiforgery();
        group.MapPut("/{id:int}", UpdateProject);
        group.MapDelete("/{id:int}", DeleteProject);
        group.MapPost("/{id:int}/employees/{employeeId:int}", AddEmployee);
        group.MapDelete("/{id:int}/employees/{employeeId:int}", RemoveEmployee);
    }

    private static async Task<IResult> GetProjects(
        [AsParameters] GetProjectsRequestDto requestDto,
        [FromServices] IProjectService projectService,
        CancellationToken cancellationToken = default)
    {
        var result = await projectService.GetProjectsAsync(requestDto, cancellationToken);

        return ResultMapper.ToActionResult(result);
    }

    private static async Task<IResult> GetProjectById(
        int id,
        [FromServices] IProjectService projectService,
        CancellationToken cancellationToken)
    {
        var result = await projectService.GetByIdAsync(id, cancellationToken);
        return ResultMapper.ToActionResult(result);
    }

    private static async Task<IResult> GetProjectDetails(
        int id,
        [FromServices] IProjectService projectService,
        CancellationToken cancellationToken)
    {
        var result = await projectService.GetProjectDetailsAsync(id, cancellationToken);
        return ResultMapper.ToActionResult(result);
    }

    private static async Task<IResult> CreateProject(
        CreateProjectDto createDto,
        [FromServices] IProjectService projectService,
        CancellationToken cancellationToken)
    {
        var result = await projectService.CreateAsync(createDto, cancellationToken);
        return ResultMapper.ToActionResult(result, successStatusCode: StatusCodes.Status201Created);
    }

    //TODO ExecutorsIds are '2,3' instead of [2,3]
    private static async Task<IResult> CreateFullProject(
        [FromForm] CreateFullProjectDto createDto,
        IFormFileCollection files,
        [FromServices] IProjectService projectService,
        CancellationToken cancellationToken)
    {
        var fileDataList = new List<FileData>();

        try
        {
            foreach (var file in files)
            {
                fileDataList.Add(new FileData(file.OpenReadStream(), file.FileName));
            }

            var result = await projectService.CreateFullAsync(createDto, fileDataList, cancellationToken);
            return ResultMapper.ToActionResult(result, successStatusCode: StatusCodes.Status201Created);
        }
        finally
        {
            foreach (var fileData in fileDataList)
            {
                await fileData.Stream.DisposeAsync();
            }
        }
    }

    private static async Task<IResult> UpdateProject(
        int id,
        UpdateProjectDto updateDto,
        [FromServices] IProjectService projectService,
        CancellationToken cancellationToken)
    {
        if (id != updateDto.Id)
            return Results.BadRequest("ID mismatch");

        var result = await projectService.UpdateAsync(updateDto, cancellationToken);
        return ResultMapper.ToActionResult(result, successStatusCode: StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> DeleteProject(
        int id,
        [FromServices] IProjectService projectService,
        CancellationToken cancellationToken)
    {
        var result = await projectService.DeleteAsync(id, cancellationToken);
        return ResultMapper.ToActionResult(result, successStatusCode: StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> AddEmployee(
        int id,
        int employeeId,
        [FromServices] IProjectService projectService,
        CancellationToken cancellationToken)
    {
        var result = await projectService.AddEmployeeAsync(id, employeeId, cancellationToken);
        return ResultMapper.ToActionResult(result, successStatusCode: StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> RemoveEmployee(
        int id,
        int employeeId,
        [FromServices] IProjectService projectService,
        CancellationToken cancellationToken)
    {
        var result = await projectService.RemoveEmployeeAsync(id, employeeId, cancellationToken);
        return ResultMapper.ToActionResult(result, successStatusCode: StatusCodes.Status204NoContent);
    }
}
