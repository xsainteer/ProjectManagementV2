using Application.DTOs.ProjectDocument;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Common;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ProjectDocumentService : IProjectDocumentService
{
    private readonly IProjectDocumentRepository _documentRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<ProjectDocumentService> _logger;

    public ProjectDocumentService(
        IProjectDocumentRepository documentRepository, 
        IProjectRepository projectRepository,
        ILogger<ProjectDocumentService> logger)
    {
        _documentRepository = documentRepository;
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async Task<Result<ProjectDocumentDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(id, true, cancellationToken);
            if (document == null)
                return Result<ProjectDocumentDto>.Failure("Document not found", 404);

            return Result<ProjectDocumentDto>.Success(MapToDto(document), 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting document with ID {DocumentId}", id);
            return Result<ProjectDocumentDto>.Failure("An internal error occurred", 500);
        }
    }

    public async Task<Result<IEnumerable<ProjectDocumentDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var documents = await _documentRepository.GetAllAsync(true, cancellationToken);
            return Result<IEnumerable<ProjectDocumentDto>>.Success(documents.Select(MapToDto), 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all documents");
            return Result<IEnumerable<ProjectDocumentDto>>.Failure("An internal error occurred", 500);
        }
    }

    public async Task<Result<ProjectDocumentDto>> CreateAsync(CreateProjectDocumentDto createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _projectRepository.GetByIdAsync(createDto.ProjectId, true, cancellationToken) == null)
                return Result<ProjectDocumentDto>.Failure("Project not found", 400);

            var document = new ProjectDocument
            {
                FileName = createDto.FileName,
                FilePath = createDto.FilePath,
                ProjectId = createDto.ProjectId
            };

            await _documentRepository.AddAsync(document, cancellationToken);
            await _documentRepository.SaveChangesAsync(cancellationToken);

            return Result<ProjectDocumentDto>.Success(MapToDto(document), 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating document");
            return Result<ProjectDocumentDto>.Failure("An internal error occurred", 500);
        }
    }

    public async Task<Result> UpdateAsync(ProjectDocumentDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(updateDto.Id, false, cancellationToken);
            if (document == null)
                return Result.Failure("Document not found", 404);

            if (document.ProjectId != updateDto.ProjectId && await _projectRepository.GetByIdAsync(updateDto.ProjectId, true, cancellationToken) == null)
                return Result.Failure("Project not found", 400);

            document.FileName = updateDto.FileName;
            document.FilePath = updateDto.FilePath;
            document.ProjectId = updateDto.ProjectId;

            _documentRepository.Update(document);
            await _documentRepository.SaveChangesAsync(cancellationToken);

            return Result.Success(204);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating document with ID {DocumentId}", updateDto.Id);
            return Result.Failure("An internal error occurred", 500);
        }
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(id, false, cancellationToken);
            if (document == null)
                return Result.Failure("Document not found", 404);

            _documentRepository.Delete(document);
            await _documentRepository.SaveChangesAsync(cancellationToken);

            return Result.Success(204);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting document with ID {DocumentId}", id);
            return Result.Failure("An internal error occurred", 500);
        }
    }

    private static ProjectDocumentDto MapToDto(ProjectDocument document) => new(
        document.Id,
        document.FileName,
        document.FilePath,
        document.ProjectId
    );
}
