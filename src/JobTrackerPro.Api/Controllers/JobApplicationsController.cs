using JobTrackerPro.Application.JobApplications.Commands;
using JobTrackerPro.Application.JobApplications.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace JobTrackerPro.Api.Controllers;

/// <summary>
/// Manages job applications — create, retrieve, update status.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class JobApplicationsController : ControllerBase
{
    private readonly ISender _sender;

    public JobApplicationsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Returns all job applications for a user.</summary>
    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetAll(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetJobApplicationsQuery(userId), cancellationToken);
        return Ok(result);
    }

    /// <summary>Creates a new job application.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateJobApplicationCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { userId = command.UserId }, new { id });
    }
}