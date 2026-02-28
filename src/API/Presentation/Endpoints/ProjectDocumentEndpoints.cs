using Application.DTOs.ProjectDocument;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Endpoints;

public static class ProjectDocumentEndpoints
{
    public static void MapProjectDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/documents").WithTags("Documents");

        group.MapGet("/", GetDocuments);
        group.MapGet("/{id:int}", GetDocumentById);
        group.MapPost("/", CreateDocument);
        group.MapPut("/{id:int}", UpdateDocument);
        group.MapDelete("/{id:int}", DeleteDocument);
    }

    private static async Task<IResult> GetDocuments(
        [FromServices] IProjectDocumentService documentService,
        CancellationToken cancellationToken)
    {
        var result = await documentService.GetAllAsync(cancellationToken);
        return ResultMapper.ToActionResult(result);
    }

    private static async Task<IResult> GetDocumentById(
        int id,
        [FromServices] IProjectDocumentService documentService,
        CancellationToken cancellationToken)
    {
        var result = await documentService.GetByIdAsync(id, cancellationToken);
        return ResultMapper.ToActionResult(result);
    }

    private static async Task<IResult> CreateDocument(
        CreateProjectDocumentDto createDto,
        [FromServices] IProjectDocumentService documentService,
        CancellationToken cancellationToken)
    {
        var result = await documentService.CreateAsync(createDto, cancellationToken);
        return ResultMapper.ToActionResult(result, successStatusCode: StatusCodes.Status201Created);
    }

    private static async Task<IResult> UpdateDocument(
        int id,
        ProjectDocumentDto updateDto,
        [FromServices] IProjectDocumentService documentService,
        CancellationToken cancellationToken)
    {
        if (id != updateDto.Id)
            return Results.BadRequest("ID mismatch");

        var result = await documentService.UpdateAsync(updateDto, cancellationToken);
        return ResultMapper.ToActionResult(result, successStatusCode: StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> DeleteDocument(
        int id,
        [FromServices] IProjectDocumentService documentService,
        CancellationToken cancellationToken)
    {
        var result = await documentService.DeleteAsync(id, cancellationToken);
        return ResultMapper.ToActionResult(result, successStatusCode: StatusCodes.Status204NoContent);
    }
}