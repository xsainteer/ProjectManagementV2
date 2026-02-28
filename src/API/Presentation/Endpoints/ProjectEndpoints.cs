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
        group.MapPost("/", CreateProject);
        group.MapPut("/{id:int}", UpdateProject);
        group.MapDelete("/{id:int}", DeleteProject);
    }

    private static async Task<IResult> GetProjects(
        [FromServices] IProjectService projectService,
        [FromQuery] DateTime? startDateFrom,
        [FromQuery] DateTime? startDateTo,
        [FromQuery] int? priority,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await projectService.GetProjectsAsync(
            startDateFrom, startDateTo, priority, sortBy, sortDescending, pageNumber, pageSize, cancellationToken);

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

    private static async Task<IResult> CreateProject(
        CreateProjectDto createDto,
        [FromServices] IProjectService projectService,
        CancellationToken cancellationToken)
    {
        var result = await projectService.CreateAsync(createDto, cancellationToken);
        return ResultMapper.ToActionResult(result, successStatusCode: StatusCodes.Status201Created);
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
}