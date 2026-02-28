using Application.DTOs.ProjectTask;
using FluentValidation;

namespace Application.Validators.ProjectTask;

public class CreateProjectTaskDtoValidator : AbstractValidator<CreateProjectTaskDto>
{
    public CreateProjectTaskDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Task name is required")
            .MaximumLength(200).WithMessage("Task name cannot exceed 200 characters");

        RuleFor(x => x.AuthorId)
            .GreaterThan(0).WithMessage("A valid author ID is required");

        RuleFor(x => x.ExecutorId)
            .GreaterThan(0).WithMessage("A valid executor ID is required");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("A valid task status is required");

        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 10).WithMessage("Priority must be between 1 and 10");

        RuleFor(x => x.ProjectId)
            .GreaterThan(0).WithMessage("A valid project ID is required");
    }
}

public class UpdateProjectTaskDtoValidator : AbstractValidator<UpdateProjectTaskDto>
{
    public UpdateProjectTaskDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("A valid task ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Task name is required")
            .MaximumLength(200).WithMessage("Task name cannot exceed 200 characters");

        RuleFor(x => x.AuthorId)
            .GreaterThan(0).WithMessage("A valid author ID is required");

        RuleFor(x => x.ExecutorId)
            .GreaterThan(0).WithMessage("A valid executor ID is required");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("A valid task status is required");

        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 10).WithMessage("Priority must be between 1 and 10");

        RuleFor(x => x.ProjectId)
            .GreaterThan(0).WithMessage("A valid project ID is required");
    }
}
