using JobTrackerPro.Application.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace JobTrackerPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Registers a new user and returns a JWT token.</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var token = await _sender.Send(command, cancellationToken);
        return Ok(new { token });
    }

    /// <summary>Authenticates a user and returns a JWT token.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var token = await _sender.Send(command, cancellationToken);
        return Ok(new { token });
    }
}
