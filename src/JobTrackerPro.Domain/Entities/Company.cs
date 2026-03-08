namespace JobTrackerPro.Domain.Entities
{
    /// <summary>
    /// Represents a company that has job openings.
    /// </summary>
    public class Company
    {
        public Guid Id { get; private set; }

        /// <summary>Company name (e.g., "NewEra Technology", "VASS").</summary>
        public string Name { get; private set; } = string.Empty;

        /// <summary>Company website URL.</summary>
        public string? Website { get; private set; }

        /// <summary>Industry or sector (e.g., "Software Development", "Consulting").</summary>
        public string? Industry { get; private set; }

        /// <summary>Company location or headquarters.</summary>
        public string? Location { get; private set; }

        /// <summary>Personal notes about the company (culture, reviews, etc.).</summary>
        public string? Notes { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        /// <summary>Navigation property: all applications to this company.</summary>
        public ICollection<JobApplication> JobApplications { get; private set; } = new List<JobApplication>();

        private Company() { }

        public static Company Create(string name, string? website = null, string? industry = null, string? location = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Company name is required.", nameof(name));

            return new Company
            {
                Id = Guid.NewGuid(),
                Name = name.Trim(),
                Website = website?.Trim(),
                Industry = industry?.Trim(),
                Location = location?.Trim(),
                CreatedAt = DateTime.UtcNow
            };
        }

        public void Update(string name, string? website, string? industry, string? location, string? notes)
        {
            Name = name.Trim();
            Website = website?.Trim();
            Industry = industry?.Trim();
            Location = location?.Trim();
            Notes = notes?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
