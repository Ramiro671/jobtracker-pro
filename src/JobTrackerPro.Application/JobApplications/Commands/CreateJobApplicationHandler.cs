using JobTrackerPro.Domain.Entities;
using JobTrackerPro.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JobTrackerPro.Application.JobApplications.Commands;

/// <summary>Handles the creation of a new job application.</summary>
public class CreateJobApplicationHandler : IRequestHandler<CreateJobApplicationCommand, Guid>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateJobApplicationHandler> _logger;

    public CreateJobApplicationHandler(
        IJobApplicationRepository jobApplicationRepository,
        ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateJobApplicationHandler> logger)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateJobApplicationCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating job application for user {UserId} at company {Company}",
            command.UserId, command.CompanyName);

        var company = await _companyRepository.GetByNameAsync(command.CompanyName, cancellationToken);
        if (company is null)
        {
            company = Company.Create(command.CompanyName);
            await _companyRepository.AddAsync(company, cancellationToken);
        }

        var application = JobApplication.Create(
            userId: command.UserId,
            title: command.Title,
            companyId: company.Id,
            jobUrl: command.JobUrl,
            description: command.Description,
            source: command.Source);

        await _jobApplicationRepository.AddAsync(application, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Job application {ApplicationId} created successfully", application.Id);

        return application.Id;
    }
}
