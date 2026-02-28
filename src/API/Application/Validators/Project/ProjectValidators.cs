using Application.DTOs.Project;
using FluentValidation;

namespace Application.Validators.Project;

public class CreateProjectDtoValidator : AbstractValidator<CreateProjectDto>
{
    public CreateProjectDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required")
            .MaximumLength(200).WithMessage("Project name cannot exceed 200 characters");

        RuleFor(x => x.CustomerCompany)
            .NotEmpty().WithMessage("Customer company name is required")
            .MaximumLength(200).WithMessage("Customer company name cannot exceed 200 characters");

        RuleFor(x => x.PerformerCompany)
            .NotEmpty().WithMessage("Performer company name is required")
            .MaximumLength(200).WithMessage("Performer company name cannot exceed 200 characters");

        RuleFor(x => x.ProjectManagerId)
            .GreaterThan(0).WithMessage("A valid project manager is required");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 10).WithMessage("Priority must be between 1 and 10");
    }
}

public class UpdateProjectDtoValidator : AbstractValidator<UpdateProjectDto>
{
    public UpdateProjectDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("A valid project ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required")
            .MaximumLength(200).WithMessage("Project name cannot exceed 200 characters");

        RuleFor(x => x.CustomerCompany)
            .NotEmpty().WithMessage("Customer company name is required")
            .MaximumLength(200).WithMessage("Customer company name cannot exceed 200 characters");

        RuleFor(x => x.PerformerCompany)
            .NotEmpty().WithMessage("Performer company name is required")
            .MaximumLength(200).WithMessage("Performer company name cannot exceed 200 characters");

        RuleFor(x => x.ProjectManagerId)
            .GreaterThan(0).WithMessage("A valid project manager is required");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 10).WithMessage("Priority must be between 1 and 10");
    }
}
