using FluentValidation;

namespace JobTrackerPro.Application.JobApplications.Commands;

/// <summary>Validates the UpdateJobApplicationCommand input.</summary>
public class UpdateJobApplicationValidator
    : AbstractValidator<UpdateJobApplicationCommand>
{
    public UpdateJobApplicationValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Application Id is required.");

        RuleFor(x => x.NewStatus)
            .IsInEnum()
            .WithMessage("Invalid application status value.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Notes must not exceed 1000 characters.");
    }
}
