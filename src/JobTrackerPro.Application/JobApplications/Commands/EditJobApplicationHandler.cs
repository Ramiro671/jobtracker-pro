using JobTrackerPro.Domain.Interfaces;
using MediatR;

namespace JobTrackerPro.Application.JobApplications.Commands;

/// <summary>Handles editing the title, URL, and notes of an existing job application.</summary>
public class EditJobApplicationHandler : IRequestHandler<EditJobApplicationCommand, bool>
{
    private readonly IJobApplicationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public EditJobApplicationHandler(IJobApplicationRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(EditJobApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (application is null)
            return false;

        application.UpdateDetails(request.Title, request.JobUrl, request.Notes);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
