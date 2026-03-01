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
        group.MapPost("/", CreateDocument).DisableAntiforgery();
        group.MapPut("/{id:int}", UpdateDocument).DisableAntiforgery();
        group.MapDelete("/{id:int}", DeleteDocument);
        group.MapGet("/{id:int}/download", DownloadDocument);
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
    
    //ENDPOINTS FOR DOCUMENTS WITH THEIR RESPECTIVE FILES

    private static async Task<IResult> CreateDocument(
        [FromForm] int projectId,
        IFormFile file,
        [FromServices] IProjectDocumentService documentService,
        CancellationToken cancellationToken)
    {
        var createDto = new CreateProjectDocumentDto(file.FileName, projectId);

        await using var stream = file.OpenReadStream();
        var result = await documentService.CreateWithFileAsync(createDto, stream, file.FileName, cancellationToken);
        
        return ResultMapper.ToActionResult(result, successStatusCode: StatusCodes.Status201Created);
    }

    private static async Task<IResult> UpdateDocument(
        int id,
        [FromForm] int projectId,
        IFormFile? file,
        [FromServices] IProjectDocumentService documentService,
        CancellationToken cancellationToken)
    {
        // For update, we might just want to update metadata or replace the file
        // If file is null, we just update metadata. 
        // Note: ProjectDocumentDto requires Id, FileName, FilePath, ProjectId.
        // We might need to get the current document first to keep existing values if not provided.
        
        var currentResult = await documentService.GetByIdAsync(id, cancellationToken);
        if (currentResult.IsFailure)
            return ResultMapper.ToActionResult(currentResult);

        var current = currentResult.Value;
        var updateDto = new ProjectDocumentDto(id, file?.FileName ?? current.FileName, current.FilePath, projectId);

        if (file != null)
        {
            await using var stream = file.OpenReadStream();
            var result = await documentService.UpdateWithFileAsync(updateDto, stream, file.FileName, cancellationToken);
            return ResultMapper.ToActionResult(result, successStatusCode: StatusCodes.Status204NoContent);
        }
        else
        {
            var result = await documentService.UpdateAsync(updateDto, cancellationToken);
            return ResultMapper.ToActionResult(result, successStatusCode: StatusCodes.Status204NoContent);
        }
    }

    private static async Task<IResult> DeleteDocument(
        int id,
        [FromServices] IProjectDocumentService documentService,
        CancellationToken cancellationToken)
    {
        var result = await documentService.DeleteAsync(id, cancellationToken);
        return ResultMapper.ToActionResult(result, successStatusCode: StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> DownloadDocument(
        int id,
        [FromServices] IProjectDocumentService documentService,
        CancellationToken cancellationToken)
    {
        var docResult = await documentService.GetByIdAsync(id, cancellationToken);
        if (docResult.IsFailure)
            return ResultMapper.ToActionResult(docResult);

        var streamResult = await documentService.GetFileStreamAsync(id, cancellationToken);
        if (streamResult.IsFailure)
            return ResultMapper.ToActionResult(streamResult);

        return Results.File(streamResult.Value, "application/octet-stream", docResult.Value.FileName);
    }
}
