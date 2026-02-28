using Application.DTOs.ProjectDocument;
using FluentValidation;

namespace Application.Validators.ProjectDocument;

public class CreateProjectDocumentDtoValidator : AbstractValidator<CreateProjectDocumentDto>
{
    public CreateProjectDocumentDtoValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required")
            .MaximumLength(255).WithMessage("File name cannot exceed 255 characters");

        RuleFor(x => x.FilePath)
            .NotEmpty().WithMessage("File path is required")
            .MaximumLength(500).WithMessage("File path cannot exceed 500 characters");

        RuleFor(x => x.ProjectId)
            .GreaterThan(0).WithMessage("A valid project ID is required");
    }
}

public class ProjectDocumentDtoValidator : AbstractValidator<ProjectDocumentDto>
{
    public ProjectDocumentDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("A valid document ID is required");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required")
            .MaximumLength(255).WithMessage("File name cannot exceed 255 characters");

        RuleFor(x => x.FilePath)
            .NotEmpty().WithMessage("File path is required")
            .MaximumLength(500).WithMessage("File path cannot exceed 500 characters");

        RuleFor(x => x.ProjectId)
            .GreaterThan(0).WithMessage("A valid project ID is required");
    }
}
