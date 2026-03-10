using JobTrackerPro.Domain.Enums;
using JobTrackerPro.Domain.ValueObjects;

namespace JobTrackerPro.Domain.Entities
{
    /// <summary>
    /// Aggregate root: represents a job application tracked by the user.
    /// Evolves from LinkedInAgent's JobOffer (Bronze) + JobOfferSilver into a
    /// personal application tracker aligned with Gold layer market insights.
    /// </summary>
    public class JobApplication
    {
        /// <summary>Updates the application status and optional notes.</summary>
public void UpdateStatus(ApplicationStatus newStatus, string? notes)
{
    Status = newStatus;
    if (notes is not null)
        Notes = notes;
    UpdatedAt = DateTime.UtcNow;
}
        public Guid Id { get; private set; }

        // ── Core fields (from Bronze layer: Url, RawText) ──

        /// <summary>Job title (e.g., "Senior .NET Developer").</summary>
        public string Title { get; private set; } = string.Empty;

        /// <summary>Job description or raw text from the posting.</summary>
        public string? Description { get; private set; }

        /// <summary>Original job posting URL (from LinkedIn, Indeed, etc.).</summary>
        public string? JobUrl { get; private set; }

        // ── Company relationship ──

        /// <summary>Foreign key to the Company entity.</summary>
        public Guid CompanyId { get; private set; }

        /// <summary>Navigation property to Company.</summary>
        public Company Company { get; private set; } = null!;

        // ── Status tracking (replaces magic strings "Crudo", "Procesado_Plata") ──

        /// <summary>Current application status in the pipeline.</summary>
        public ApplicationStatus Status { get; private set; } = ApplicationStatus.Saved;

        /// <summary>Seniority level of the position (from Silver layer analysis).</summary>
        public SeniorityLevel SeniorityLevel { get; private set; } = SeniorityLevel.NotSpecified;

        /// <summary>Work modality: Remote, Hybrid, or OnSite.</summary>
        public WorkModality WorkModality { get; private set; } = WorkModality.NotSpecified;

        // ── Tech stack (aligned with Gold layer categories) ──

        /// <summary>
        /// Technology stack required for this position.
        /// Categories: Backend, Frontend, Databases, CloudAndDevOps, Testing.
        /// </summary>
        public TechStack TechStack { get; private set; } = TechStack.Create();

        // ── Salary & compensation ──

        /// <summary>Minimum salary offered (if disclosed).</summary>
        public decimal? SalaryMin { get; private set; }

        /// <summary>Maximum salary offered (if disclosed).</summary>
        public decimal? SalaryMax { get; private set; }

        /// <summary>Currency code (e.g., "USD", "MXN").</summary>
        public string? SalaryCurrency { get; private set; }

        // ── Contact info ──

        /// <summary>Recruiter or hiring manager name.</summary>
        public string? ContactName { get; private set; }

        /// <summary>Recruiter or hiring manager email.</summary>
        public string? ContactEmail { get; private set; }

        /// <summary>Source platform (e.g., "LinkedIn", "Indeed", "Referral").</summary>
        public string? Source { get; private set; }

        // ── Dates ──

        /// <summary>Date the user saved or found this job.</summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>Date the user actually applied.</summary>
        public DateTime? AppliedAt { get; private set; }

        /// <summary>Last time any field was updated.</summary>
        public DateTime? UpdatedAt { get; private set; }

        // ── User notes ──

        /// <summary>Personal notes, interview prep, follow-up reminders.</summary>
        public string? Notes { get; private set; }

        // ── User relationship ──

        /// <summary>Foreign key to the user who owns this application.</summary>
        public Guid UserId { get; private set; }

        // ── Private constructor for EF Core ──
        private JobApplication() { }

        // ── Factory method ──

        /// <summary>
        /// Creates a new job application with required fields.
        /// </summary>
        public static JobApplication Create(
            Guid userId,
            string title,
            Guid companyId,
            string? jobUrl = null,
            string? description = null,
            string? source = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Job title is required.", nameof(title));

            return new JobApplication
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title.Trim(),
                CompanyId = companyId,
                JobUrl = jobUrl?.Trim(),
                Description = description?.Trim(),
                Source = source?.Trim(),
                Status = ApplicationStatus.Saved,
                CreatedAt = DateTime.UtcNow
            };
        }

        // ── Behavior methods (Domain logic) ──

        /// <summary>
        /// Advances the application status in the pipeline.
        /// </summary>
        public void UpdateStatus(ApplicationStatus newStatus)
        {
            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;

            if (newStatus == ApplicationStatus.Applied && !AppliedAt.HasValue)
                AppliedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Sets the technology stack for this position.
        /// </summary>
        public void SetTechStack(TechStack techStack)
        {
            TechStack = techStack ?? throw new ArgumentNullException(nameof(techStack));
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates seniority level and work modality.
        /// </summary>
        public void SetJobDetails(SeniorityLevel seniority, WorkModality modality)
        {
            SeniorityLevel = seniority;
            WorkModality = modality;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Sets salary range information.
        /// </summary>
        public void SetSalary(decimal? min, decimal? max, string? currency)
        {
            if (min.HasValue && max.HasValue && min > max)
                throw new ArgumentException("Minimum salary cannot exceed maximum.");

            SalaryMin = min;
            SalaryMax = max;
            SalaryCurrency = currency?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Sets recruiter/hiring manager contact information.
        /// </summary>
        public void SetContact(string? name, string? email)
        {
            ContactName = name?.Trim();
            ContactEmail = email?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates personal notes for this application.
        /// </summary>
        public void SetNotes(string? notes)
        {
            Notes = notes?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
