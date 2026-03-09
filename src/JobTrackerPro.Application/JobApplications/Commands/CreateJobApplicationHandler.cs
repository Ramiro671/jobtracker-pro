using JobTrackerPro.Domain.Entities;
using JobTrackerPro.Domain.Interfaces;
using MediatR;

namespace JobTrackerPro.Application.JobApplications.Commands;

/// <summary>Handles the creation of a new job application.</summary>
public class CreateJobApplicationHandler : IRequestHandler<CreateJobApplicationCommand, Guid>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly ICompanyRepository _companyRepository;

    public CreateJobApplicationHandler(
        IJobApplicationRepository jobApplicationRepository,
        ICompanyRepository companyRepository)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _companyRepository = companyRepository;
    }

    public async Task<Guid> Handle(CreateJobApplicationCommand command, CancellationToken cancellationToken)
    {
        // Get or create the company
        var company = await _companyRepository.GetByNameAsync(command.CompanyName, cancellationToken);
        if (company is null)
        {
            company = Company.Create(command.CompanyName);
            await _companyRepository.AddAsync(company, cancellationToken);
        }

        // Create the job application
        var application = JobApplication.Create(
            userId: command.UserId,
            title: command.Title,
            companyId: company.Id,
            jobUrl: command.JobUrl,
            description: command.Description,
            source: command.Source);

        await _jobApplicationRepository.AddAsync(application, cancellationToken);

        return application.Id;
    }
}