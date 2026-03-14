using JobTrackerPro.Application.Common.Interfaces;
using JobTrackerPro.Domain.Enums;
using JobTrackerPro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobTrackerPro.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that runs once every 24 hours and sends an email
/// to users who have job applications in active statuses with no activity
/// for 7+ days.
///
/// Requires EmailSettings:Enabled = true and valid SMTP credentials.
/// When disabled, emails are logged but not sent.
/// </summary>
public class StaleNotificationService : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(24);
    private static readonly int StaleDays = 7;

    // Backend enum values that map to "active" states visible on the frontend
    private static readonly ApplicationStatus[] ActiveStatuses =
    [
        ApplicationStatus.Saved,        // frontend: Applied (0)
        ApplicationStatus.Applied,      // frontend: Phone Screen (1)
        ApplicationStatus.Screening,    // frontend: Interview (2)
        ApplicationStatus.TechnicalTest,// frontend: Technical Test (3)
        ApplicationStatus.Interview,    // frontend: Final Interview (4)
    ];

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StaleNotificationService> _logger;

    public StaleNotificationService(
        IServiceScopeFactory scopeFactory,
        ILogger<StaleNotificationService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("StaleNotificationService started. Interval: {Interval}h", CheckInterval.TotalHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndNotifyAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in StaleNotificationService");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task CheckAndNotifyAsync(CancellationToken ct)
    {
        using var scope   = _scopeFactory.CreateScope();
        var db            = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var emailService  = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var cutoff = DateTime.UtcNow.AddDays(-StaleDays);

        var staleApps = await db.JobApplications
            .Where(a => ActiveStatuses.Contains(a.Status))
            .Where(a => (a.UpdatedAt ?? a.CreatedAt) < cutoff)
            .Include(a => a.Company)
            .ToListAsync(ct);

        if (!staleApps.Any())
        {
            _logger.LogInformation("StaleNotificationService: no stale applications found.");
            return;
        }

        // Fetch users separately — JobApplication has UserId FK but no User nav property
        var userIds = staleApps.Select(a => a.UserId).Distinct().ToList();
        var users = await db.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, ct);

        var byUser = staleApps.GroupBy(a => a.UserId);
        foreach (var group in byUser)
        {
            if (!users.TryGetValue(group.Key, out var user)) continue;

            var appLines = string.Join("", group.Select(a =>
                $"<li><strong>{a.Title}</strong> at <em>{a.Company.Name}</em></li>"));

            var body = $"""
                <p>Hi {user.FullName},</p>
                <p>The following job applications haven't had any activity in {StaleDays}+ days:</p>
                <ul>{appLines}</ul>
                <p>Consider following up or updating their status in <a href="https://gleaming-lollipop-3b4183.netlify.app">JobTracker Pro</a>.</p>
                <p style="color:#888;font-size:12px">You're receiving this because you have active applications tracked in JobTracker Pro.</p>
                """;

            await emailService.SendAsync(
                to: user.Email,
                subject: $"JobTracker Pro - {group.Count()} application(s) need attention",
                htmlBody: body,
                cancellationToken: ct);
        }

        _logger.LogInformation(
            "StaleNotificationService: notified {UserCount} user(s) about {AppCount} stale application(s).",
            byUser.Count(), staleApps.Count);
    }
}
