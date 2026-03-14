namespace JobTrackerPro.Application.Common.Interfaces;

/// <summary>Sends transactional emails. Implementations must be thread-safe.</summary>
public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
