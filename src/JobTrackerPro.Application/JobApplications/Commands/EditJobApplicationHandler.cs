using JobTrackerPro.Domain.Entities;
using JobTrackerPro.Domain.Interfaces;
using MediatR;

namespace JobTrackerPro.Application.JobApplications.Commands;

/// <summary>Handles editing the title, company, URL, and notes of an existing job application.</summary>
public class EditJobApplicationHandler : IRequestHandler<EditJobApplicationCommand, bool>
{
    private readonly IJobApplicationRepository _repository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EditJobApplicationHandler(
        IJobApplicationRepository repository,
        ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(EditJobApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (application is null)
            return false;

        application.UpdateDetails(request.Title, request.JobUrl, request.Notes);

        // Reassign company if name changed
        if (!string.IsNullOrWhiteSpace(request.CompanyName))
        {
            var company = await _companyRepository.GetByNameAsync(request.CompanyName, cancellationToken);
            if (company is null)
            {
                company = Company.Create(request.CompanyName);
                await _companyRepository.AddAsync(company, cancellationToken);
            }
            application.UpdateCompany(company.Id);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
