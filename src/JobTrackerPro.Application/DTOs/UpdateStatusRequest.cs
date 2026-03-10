using JobTrackerPro.Domain.Enums;

namespace JobTrackerPro.Application.DTOs;

/// <summary>Request payload for updating a job application status.</summary>
public record UpdateStatusRequest(
    ApplicationStatus NewStatus,
    string? Notes
);