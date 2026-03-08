namespace JobTrackerPro.Domain.Entities
{
    /// <summary>
    /// Represents a registered user of the JobTracker Pro application.
    /// Supports JWT authentication (Phase 1) and future OAuth2 integration.
    /// </summary>
    public class User
    {
        public Guid Id { get; private set; }

        /// <summary>User's full name.</summary>
        public string FullName { get; private set; } = string.Empty;

        /// <summary>User's email address (unique, used for login).</summary>
        public string Email { get; private set; } = string.Empty;

        /// <summary>Hashed password (never stored in plain text).</summary>
        public string PasswordHash { get; private set; } = string.Empty;

        public DateTime CreatedAt { get; private set; }
        public DateTime? LastLoginAt { get; private set; }

        /// <summary>Navigation property: all job applications owned by this user.</summary>
        public ICollection<JobApplication> JobApplications { get; private set; } = new List<JobApplication>();

        private User() { }

        public static User Create(string fullName, string email, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name is required.", nameof(fullName));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.", nameof(email));

            return new User
            {
                Id = Guid.NewGuid(),
                FullName = fullName.Trim(),
                Email = email.Trim().ToLowerInvariant(),
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void RecordLogin()
        {
            LastLoginAt = DateTime.UtcNow;
        }
    }
}
