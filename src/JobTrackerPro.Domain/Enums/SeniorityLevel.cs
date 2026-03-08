namespace JobTrackerPro.Domain.Enums
{
    /// <summary>
    /// Seniority level extracted from job postings.
    /// Based on the 1,319 LinkedIn offers analyzed in the Gold layer.
    /// </summary>
    public enum SeniorityLevel
    {
        NotSpecified = 0,
        Intern = 1,
        Junior = 2,
        Mid = 3,
        Senior = 4,
        Lead = 5,
        Staff = 6,
        Principal = 7
    }
}
