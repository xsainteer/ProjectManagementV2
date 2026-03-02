using Application.DTOs.ProjectDocument;
using Application.Interfaces;
using Application.Interfaces.Services;
using Application.Mappings;
using Application.Validators;
using Domain.Common;
using Domain.Entities;
using FluentValidation;

namespace Application.Services;

public class ProjectDocumentService : IProjectDocumentService
{
    private readonly IProjectDocumentRepository _documentRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IFileService _fileService;
    private readonly IValidator<CreateProjectDocumentDto> _createValidator;
    private readonly IValidator<ProjectDocumentDto> _updateValidator;

    public ProjectDocumentService(
        IProjectDocumentRepository documentRepository, 
        IProjectRepository projectRepository,
        IFileService fileService,
        IValidator<CreateProjectDocumentDto> createValidator,
        IValidator<ProjectDocumentDto> updateValidator)
    {
        _documentRepository = documentRepository;
        _projectRepository = projectRepository;
        _fileService = fileService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<Result<ProjectDocumentDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(id, true, cancellationToken);
        if (document == null)
            return Result<ProjectDocumentDto>.Failure(Error.NotFound($"Document with ID {id} was not found."));

        return Result<ProjectDocumentDto>.Success(document.ToDto());
    }

    public async Task<Result<IEnumerable<ProjectDocumentDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _documentRepository.GetAllAsync(true, cancellationToken);
        return Result<IEnumerable<ProjectDocumentDto>>.Success(documents.Select(d => d.ToDto()));
    }

    public async Task<Result<ProjectDocumentDto>> CreateAsync(CreateProjectDocumentDto createDto, CancellationToken cancellationToken = default)
    {
        return await CreateWithFileInternalAsync(createDto, null, null, cancellationToken);
    }

    public async Task<Result<ProjectDocumentDto>> CreateWithFileAsync(CreateProjectDocumentDto createDto, Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        return await CreateWithFileInternalAsync(createDto, fileStream, fileName, cancellationToken);
    }

    private async Task<Result<ProjectDocumentDto>> CreateWithFileInternalAsync(CreateProjectDocumentDto createDto, Stream? fileStream, string? fileName, CancellationToken cancellationToken)
    {
        var validation = await _createValidator.ValidateToResultAsync(createDto, cancellationToken);
        if (validation.IsFailure) return Result<ProjectDocumentDto>.Failure(validation.Error);

        if (await _projectRepository.GetByIdAsync(createDto.ProjectId, true, cancellationToken) == null)
            return Result<ProjectDocumentDto>.Failure(Error.NotFound($"Project with ID {createDto.ProjectId} was not found."));

        await using var transaction = await _documentRepository.BeginTransactionAsync(cancellationToken);
        string savedFilePath = String.Empty;

        try
        {
            if (fileStream != null && fileName != null)
            {
                var saveResult = await _fileService.SaveFileAsync(fileStream, fileName, $"project_{createDto.ProjectId}", cancellationToken);
                if (saveResult.IsFailure)
                    return Result<ProjectDocumentDto>.Failure(saveResult.Error);
                
                savedFilePath = saveResult.Value;
            }

            var document = createDto.ToEntity(fileName, savedFilePath);

            await _documentRepository.AddAsync(document, cancellationToken);
            await _documentRepository.SaveChangesAsync(cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);

            return Result<ProjectDocumentDto>.Success(document.ToDto());
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            _fileService.DeleteFile(savedFilePath);
            throw;
        }
    }

    public async Task<Result> UpdateAsync(ProjectDocumentDto updateDto, CancellationToken cancellationToken = default)
    {
        return await UpdateWithFileAsync(updateDto, null, null, cancellationToken);
    }

    public async Task<Result> UpdateWithFileAsync(ProjectDocumentDto updateDto, Stream? fileStream, string? fileName, CancellationToken cancellationToken = default)
    {
        var validation = await _updateValidator.ValidateToResultAsync(updateDto, cancellationToken);
        if (validation.IsFailure) return validation;

        var document = await _documentRepository.GetByIdAsync(updateDto.Id, false, cancellationToken);
        if (document == null)
            return Result.Failure(Error.NotFound($"Document with ID {updateDto.Id} was not found."));

        if (document.ProjectId != updateDto.ProjectId && await _projectRepository.GetByIdAsync(updateDto.ProjectId, true, cancellationToken) == null)
            return Result.Failure(Error.NotFound($"Project with ID {updateDto.ProjectId} was not found."));

        await using var transaction = await _documentRepository.BeginTransactionAsync(cancellationToken);
        string? oldFilePath = document.FilePath;
        string? newFilePath = null;

        try
        {
            if (fileStream != null && fileName != null)
            {
                var saveResult = await _fileService.SaveFileAsync(fileStream, fileName, $"project_{updateDto.ProjectId}", cancellationToken);
                if (saveResult.IsFailure)
                    return Result.Failure(saveResult.Error);
                
                newFilePath = saveResult.Value;
                document.FileName = fileName;
                document.FilePath = newFilePath;
                document.ProjectId = updateDto.ProjectId;
            }
            else
            {
                document.UpdateWith(updateDto);
            }

            _documentRepository.Update(document);
            await _documentRepository.SaveChangesAsync(cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);

            if (newFilePath != null && oldFilePath != null && oldFilePath != newFilePath)
            {
                _fileService.DeleteFile(oldFilePath);
            }

            return Result.Success();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            if (newFilePath != null) _fileService.DeleteFile(newFilePath);
            throw;
        }
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(id, false, cancellationToken);
        if (document == null)
            return Result.Failure(Error.NotFound($"Document with ID {id} was not found."));

        var filePath = document.FilePath;
        await using var transaction = await _documentRepository.BeginTransactionAsync(cancellationToken);

        try
        {
            _documentRepository.Delete(document);
            await _documentRepository.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            if (!string.IsNullOrEmpty(filePath)) _fileService.DeleteFile(filePath);

            return Result.Success();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<Stream>> GetFileStreamAsync(int id, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(id, true, cancellationToken);
        if (document == null)
            return Result<Stream>.Failure(Error.NotFound($"Document with ID {id} was not found."));

        return _fileService.GetFileStream(document.FilePath);
    }
}
