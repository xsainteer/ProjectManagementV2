using Application.DTOs.ProjectTask;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Endpoints;

public static class ProjectTaskEndpoints
{
    public static void MapProjectTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks").WithTags("Tasks");

        group.MapGet("/", GetTasks);
        group.MapGet("/{id:int}", GetTaskById);
        group.MapPost("/", CreateTask);
        group.MapPut("/{id:int}", UpdateTask);
        group.MapDelete("/{id:int}", DeleteTask);
    }

    private static async Task<IResult> GetTasks(
        [FromServices] IProjectTaskService taskService,
        CancellationToken cancellationToken)
    {
        var result = await taskService.GetAllAsync(cancellationToken);
        return ResultMapper.ToActionResult(result);
    }

    private static async Task<IResult> GetTaskById(
        int id,
        [FromServices] IProjectTaskService taskService,
        CancellationToken cancellationToken)
    {
        var result = await taskService.GetByIdAsync(id, cancellationToken);
        return ResultMapper.ToActionResult(result);
    }

    private static async Task<IResult> CreateTask(
        CreateProjectTaskDto createDto,
        [FromServices] IProjectTaskService taskService,
        CancellationToken cancellationToken)
    {
        var result = await taskService.CreateAsync(createDto, cancellationToken);
        return ResultMapper.ToActionResult(result, successStatusCode: StatusCodes.Status201Created);
    }

    private static async Task<IResult> UpdateTask(
        int id,
        UpdateProjectTaskDto updateDto,
        [FromServices] IProjectTaskService taskService,
        CancellationToken cancellationToken)
    {
        if (id != updateDto.Id)
            return Results.BadRequest("ID mismatch");

        var result = await taskService.UpdateAsync(updateDto, cancellationToken);
        return ResultMapper.ToActionResult(result, successStatusCode: StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> DeleteTask(
        int id,
        [FromServices] IProjectTaskService taskService,
        CancellationToken cancellationToken)
    {
        var result = await taskService.DeleteAsync(id, cancellationToken);
        return ResultMapper.ToActionResult(result, successStatusCode: StatusCodes.Status204NoContent);
    }
}