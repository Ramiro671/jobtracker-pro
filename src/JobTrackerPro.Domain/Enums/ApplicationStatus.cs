namespace JobTrackerPro.Domain.Enums
{
    /// <summary>
    /// Represents the current stage of a job application in the pipeline.
    /// Inspired by the Medallion Architecture flow: Raw → Processed → Actionable.
    /// </summary>
    public enum ApplicationStatus
    {
        /// <summary>Job saved but not yet applied.</summary>
        Saved = 0,

        /// <summary>Application submitted to the company.</summary>
        Applied = 1,

        /// <summary>Recruiter/HR acknowledged or screening in progress.</summary>
        Screening = 2,

        /// <summary>Technical assessment or coding challenge assigned.</summary>
        TechnicalTest = 3,

        /// <summary>Interview scheduled or completed (phone, video, onsite).</summary>
        Interview = 4,

        /// <summary>Offer received from the company.</summary>
        OfferReceived = 5,

        /// <summary>Offer accepted by the candidate.</summary>
        Accepted = 6,

        /// <summary>Application rejected by the company.</summary>
        Rejected = 7,

        /// <summary>Application withdrawn by the candidate.</summary>
        Withdrawn = 8
    }
}
