namespace Infrastructure.DermaImage.Services.Emailing;

public class EmailSettings
{
    public string SmtpServerAddress { get; set; } = "";
    public int SmtpServerPort { get; set; }
    public string SmtpUserName { get; set; } = "";
    public string SmtpPassword { get; set; } = "";
    public bool EnableSSL { get; set; }
    public string EmailAddress { get; set; } = "";
    /// <summary>
    /// The name that appear when a user receives an email from the application
    /// </summary>
    public string EmailAddressDisplay { get; set; } = "";

    public string EnvironmentSubjectPrefix { get; init; } = "";
}
