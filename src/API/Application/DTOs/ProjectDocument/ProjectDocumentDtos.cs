namespace Application.DTOs.ProjectDocument;

public record ProjectDocumentDto(
    int Id,
    string FileName,
    string FilePath,
    int ProjectId
);

public record CreateProjectDocumentDto(
    string FileName,
    int ProjectId
);
