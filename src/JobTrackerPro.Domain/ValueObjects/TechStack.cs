namespace JobTrackerPro.Domain.ValueObjects
{
    /// <summary>
    /// Represents the technology stack required for a job position.
    /// Categories aligned with Gold layer analysis:
    ///   Backend: C#/.NET Core, ASP.NET Core, Entity Framework, Java, Python, Node.js
    ///   Frontend: JavaScript, React, Angular, TypeScript, HTML/CSS
    ///   Database: SQL Server, PostgreSQL, MongoDB, Redis, Oracle
    ///   Cloud/DevOps: AWS, Azure, Docker, Kubernetes, Git, CI/CD, Terraform
    ///   Testing: XUnit, NUnit, Selenium, Cypress, Playwright
    /// </summary>
    public class TechStack
    {
        /// <summary>Backend technologies (e.g., "C#, .NET Core, ASP.NET Core, Entity Framework").</summary>
        public string Backend { get; private set; } = string.Empty;

        /// <summary>Frontend technologies (e.g., "React, TypeScript, Tailwind CSS").</summary>
        public string Frontend { get; private set; } = string.Empty;

        /// <summary>Database technologies (e.g., "PostgreSQL, Redis").</summary>
        public string Databases { get; private set; } = string.Empty;

        /// <summary>Cloud and DevOps tools (e.g., "Azure, Docker, GitHub Actions").</summary>
        public string CloudAndDevOps { get; private set; } = string.Empty;

        /// <summary>Testing frameworks (e.g., "xUnit, FluentAssertions, Playwright").</summary>
        public string Testing { get; private set; } = string.Empty;

        /// <summary>Required for EF Core.</summary>
        private TechStack() { }

        /// <summary>
        /// Creates a new TechStack value object with categorized technologies.
        /// </summary>
        public static TechStack Create(
            string backend = "",
            string frontend = "",
            string databases = "",
            string cloudAndDevOps = "",
            string testing = "")
        {
            return new TechStack
            {
                Backend = backend.Trim(),
                Frontend = frontend.Trim(),
                Databases = databases.Trim(),
                CloudAndDevOps = cloudAndDevOps.Trim(),
                Testing = testing.Trim()
            };
        }

        /// <summary>
        /// Creates a TechStack from a flat comma-separated string (legacy Silver layer format).
        /// </summary>
        public static TechStack FromFlatString(string technologies, string frameworks)
        {
            return new TechStack
            {
                Backend = technologies.Trim(),
                Frontend = frameworks.Trim()
            };
        }
    }
}
