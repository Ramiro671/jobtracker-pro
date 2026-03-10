using JobTrackerPro.Domain.Interfaces;
using MediatR;

namespace JobTrackerPro.Application.JobApplications.Commands;

public class UpdateJobApplicationHandler
    : IRequestHandler<UpdateJobApplicationCommand, bool>
{
    private readonly IJobApplicationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateJobApplicationHandler(
        IJobApplicationRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        UpdateJobApplicationCommand request,
        CancellationToken cancellationToken)
    {
        var application = await _repository
            .GetByIdAsync(request.Id, cancellationToken);

        if (application is null)
            return false;

        application.UpdateStatus(request.NewStatus, request.Notes);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}