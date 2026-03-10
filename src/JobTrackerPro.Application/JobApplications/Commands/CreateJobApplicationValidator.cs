using FluentValidation;

namespace JobTrackerPro.Application.JobApplications.Commands;

/// <summary>Validates the CreateJobApplicationCommand input.</summary>
public class CreateJobApplicationValidator
    : AbstractValidator<CreateJobApplicationCommand>
{
    public CreateJobApplicationValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Job title is required.")
            .MaximumLength(200)
            .WithMessage("Job title must not exceed 200 characters.");

        RuleFor(x => x.CompanyName)
            .NotEmpty()
            .WithMessage("Company name is required.")
            .MaximumLength(100)
            .WithMessage("Company name must not exceed 100 characters.");

        RuleFor(x => x.JobUrl)
            .MaximumLength(500)
            .WithMessage("Job URL must not exceed 500 characters.")
            .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Job URL must be a valid URL.");

        RuleFor(x => x.Source)
            .NotEmpty()
            .WithMessage("Source is required.")
            .MaximumLength(50)
            .WithMessage("Source must not exceed 50 characters.");
    }
}
