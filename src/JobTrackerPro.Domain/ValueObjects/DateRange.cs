namespace JobTrackerPro.Domain.ValueObjects
{
    /// <summary>
    /// Represents a date range with start and optional end date.
    /// Used for interview periods, application windows, etc.
    /// </summary>
    public class DateRange
    {
        public DateTime Start { get; private set; }
        public DateTime? End { get; private set; }

        private DateRange() { }

        public static DateRange Create(DateTime start, DateTime? end = null)
        {
            if (end.HasValue && end.Value < start)
                throw new ArgumentException("End date cannot be before start date.");

            return new DateRange { Start = start, End = end };
        }

        public int DaysElapsed => (int)((End ?? DateTime.UtcNow) - Start).TotalDays;
    }
}
