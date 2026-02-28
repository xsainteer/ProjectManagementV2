using Application.DTOs.ProjectDocument;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Common;
using Domain.Entities;

namespace Application.Services;

public class ProjectDocumentService : IProjectDocumentService
{
    private readonly IProjectDocumentRepository _documentRepository;
    private readonly IProjectRepository _projectRepository;

    public ProjectDocumentService(IProjectDocumentRepository documentRepository, IProjectRepository projectRepository)
    {
        _documentRepository = documentRepository;
        _projectRepository = projectRepository;
    }

    public async Task<Result<ProjectDocumentDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(id, true, cancellationToken);
        if (document == null)
            return Result<ProjectDocumentDto>.Failure("Document not found");

        return Result<ProjectDocumentDto>.Success(MapToDto(document));
    }

    public async Task<Result<IEnumerable<ProjectDocumentDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _documentRepository.GetAllAsync(true, cancellationToken);
        return Result<IEnumerable<ProjectDocumentDto>>.Success(documents.Select(MapToDto));
    }

    public async Task<Result<ProjectDocumentDto>> CreateAsync(CreateProjectDocumentDto createDto, CancellationToken cancellationToken = default)
    {
        if (await _projectRepository.GetByIdAsync(createDto.ProjectId, true, cancellationToken) == null)
            return Result<ProjectDocumentDto>.Failure("Project not found");

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

    public async Task<Result> UpdateAsync(ProjectDocumentDto updateDto, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(updateDto.Id, false, cancellationToken);
        if (document == null)
            return Result.Failure("Document not found");

        if (document.ProjectId != updateDto.ProjectId && await _projectRepository.GetByIdAsync(updateDto.ProjectId, true, cancellationToken) == null)
            return Result.Failure("Project not found");

        document.FileName = updateDto.FileName;
        document.FilePath = updateDto.FilePath;
        document.ProjectId = updateDto.ProjectId;

        _documentRepository.Update(document);
        await _documentRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(id, false, cancellationToken);
        if (document == null)
            return Result.Failure("Document not found");

        _documentRepository.Delete(document);
        await _documentRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static ProjectDocumentDto MapToDto(ProjectDocument document) => new(
        document.Id,
        document.FileName,
        document.FilePath,
        document.ProjectId
    );
}
