using JobTrackerPro.Application.DTOs;
using JobTrackerPro.Application.JobApplications.Commands;
using JobTrackerPro.Application.JobApplications.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using JobTrackerPro.Domain.Enums;

namespace JobTrackerPro.Api.Controllers;

/// <summary>
/// Manages job applications — create, retrieve, update status.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("api")]
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

    /// <summary>Updates the status of a job application.</summary>
[HttpPut("{id:guid}")]
public async Task<IActionResult> UpdateStatus(
    Guid id,
    [FromBody] UpdateStatusRequest request,
    CancellationToken cancellationToken)
{
    var result = await _sender.Send(
        new UpdateJobApplicationCommand(id, request.NewStatus, request.Notes),
        cancellationToken);

    return result ? NoContent() : NotFound();
}

/// <summary>Edits title, job URL, and notes of a job application.</summary>
[HttpPatch("{id:guid}")]
public async Task<IActionResult> Edit(
    Guid id,
    [FromBody] EditJobApplicationCommand command,
    CancellationToken cancellationToken)
{
    var result = await _sender.Send(command with { Id = id }, cancellationToken);
    return result ? NoContent() : NotFound();
}

/// <summary>Deletes a job application.</summary>
[HttpDelete("{id:guid}")]
public async Task<IActionResult> Delete(
    Guid id,
    CancellationToken cancellationToken)
{
    var result = await _sender.Send(
        new DeleteJobApplicationCommand(id),
        cancellationToken);

    return result ? NoContent() : NotFound();
}
}