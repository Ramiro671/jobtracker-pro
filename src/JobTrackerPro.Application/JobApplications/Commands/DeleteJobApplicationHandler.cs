using JobTrackerPro.Domain.Interfaces;
using MediatR;

namespace JobTrackerPro.Application.JobApplications.Commands;

/// <summary>Handles deletion of a job application by its identifier.</summary>
public class DeleteJobApplicationHandler
    : IRequestHandler<DeleteJobApplicationCommand, bool>
{
    private readonly IJobApplicationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteJobApplicationHandler(
        IJobApplicationRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        DeleteJobApplicationCommand request,
        CancellationToken cancellationToken)
    {
        var application = await _repository
            .GetByIdAsync(request.Id, cancellationToken);

        if (application is null)
            return false;

        _repository.Delete(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}