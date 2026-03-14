namespace JobTrackerPro.Infrastructure.Email;

public class EmailSettings
{
    public bool   Enabled   { get; set; } = false;
    public string SmtpHost  { get; set; } = "smtp.gmail.com";
    public int    SmtpPort  { get; set; } = 587;
    public string Username  { get; set; } = string.Empty;
    public string Password  { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "noreply@jobtrackerpro.dev";
    public string FromName  { get; set; } = "JobTracker Pro";
}
