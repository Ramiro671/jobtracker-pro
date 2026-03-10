namespace JobTrackerPro.Application.DTOs;

/// <summary>Data Transfer Object for returning job application data to the client.</summary>
public record JobApplicationDto(
    Guid Id,
    string Title,
    string CompanyName,
    string Status,
    string WorkModality,
    string SeniorityLevel,
    string? JobUrl,
    string? Source,
    decimal? SalaryMin,
    decimal? SalaryMax,
    string? SalaryCurrency,
    string? Notes,
    DateTime CreatedAt,
    DateTime? AppliedAt
    
);
