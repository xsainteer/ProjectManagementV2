using Application.DTOs.ProjectDocument;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Common;
using Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ProjectDocumentService : IProjectDocumentService
{
    private readonly IProjectDocumentRepository _documentRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<ProjectDocumentService> _logger;
    private readonly IValidator<CreateProjectDocumentDto> _createValidator;
    private readonly IValidator<ProjectDocumentDto> _updateValidator;

    public ProjectDocumentService(
        IProjectDocumentRepository documentRepository, 
        IProjectRepository projectRepository,
        ILogger<ProjectDocumentService> logger,
        IValidator<CreateProjectDocumentDto> createValidator,
        IValidator<ProjectDocumentDto> updateValidator)
    {
        _documentRepository = documentRepository;
        _projectRepository = projectRepository;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<Result<ProjectDocumentDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(id, true, cancellationToken);
            if (document == null)
                return Result<ProjectDocumentDto>.Failure(Error.NotFound($"Document with ID {id} was not found."));

            return Result<ProjectDocumentDto>.Success(MapToDto(document));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting document with ID {DocumentId}", id);
            return Result<ProjectDocumentDto>.Failure(Error.Unexpected("An internal error occurred"));
        }
    }

    public async Task<Result<IEnumerable<ProjectDocumentDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var documents = await _documentRepository.GetAllAsync(true, cancellationToken);
            return Result<IEnumerable<ProjectDocumentDto>>.Success(documents.Select(MapToDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all documents");
            return Result<IEnumerable<ProjectDocumentDto>>.Failure(Error.Unexpected("An internal error occurred"));
        }
    }

    public async Task<Result<ProjectDocumentDto>> CreateAsync(CreateProjectDocumentDto createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = await _createValidator.ValidateAsync(createDto, cancellationToken);
            if (!validationResult.IsValid)
                return Result<ProjectDocumentDto>.Failure(Error.Validation(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));

            if (await _projectRepository.GetByIdAsync(createDto.ProjectId, true, cancellationToken) == null)
                return Result<ProjectDocumentDto>.Failure(Error.NotFound($"Project with ID {createDto.ProjectId} was not found."));

            var document = new ProjectDocument
            {
                FileName = createDto.FileName,
                FilePath = createDto.FilePath,
                ProjectId = createDto.ProjectId
            };

            await _documentRepository.AddAsync(document, cancellationToken);
            await _documentRepository.SaveChangesAsync(cancellationToken);

            return Result<ProjectDocumentDto>.Success(MapToDto(document));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating document");
            return Result<ProjectDocumentDto>.Failure(Error.Unexpected("An internal error occurred"));
        }
    }

    public async Task<Result> UpdateAsync(ProjectDocumentDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = await _updateValidator.ValidateAsync(updateDto, cancellationToken);
            if (!validationResult.IsValid)
                return Result.Failure(Error.Validation(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))));

            var document = await _documentRepository.GetByIdAsync(updateDto.Id, false, cancellationToken);
            if (document == null)
                return Result.Failure(Error.NotFound($"Document with ID {updateDto.Id} was not found."));

            if (document.ProjectId != updateDto.ProjectId && await _projectRepository.GetByIdAsync(updateDto.ProjectId, true, cancellationToken) == null)
                return Result.Failure(Error.NotFound($"Project with ID {updateDto.ProjectId} was not found."));

            document.FileName = updateDto.FileName;
            document.FilePath = updateDto.FilePath;
            document.ProjectId = updateDto.ProjectId;

            _documentRepository.Update(document);
            await _documentRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating document with ID {DocumentId}", updateDto.Id);
            return Result.Failure(Error.Unexpected("An internal error occurred"));
        }
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(id, false, cancellationToken);
            if (document == null)
                return Result.Failure(Error.NotFound($"Document with ID {id} was not found."));

            _documentRepository.Delete(document);
            await _documentRepository.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting document with ID {DocumentId}", id);
            return Result.Failure(Error.Unexpected("An internal error occurred"));
        }
    }

    private static ProjectDocumentDto MapToDto(ProjectDocument document) => new(
        document.Id,
        document.FileName,
        document.FilePath,
        document.ProjectId
    );
}