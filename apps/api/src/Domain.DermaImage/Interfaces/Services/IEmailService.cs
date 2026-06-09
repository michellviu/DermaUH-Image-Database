namespace Domain.DermaImage.Interfaces.Services;

/// <summary>
/// Abstraction for sending transactional emails.
/// </summary>
public interface IEmailService
{
    Task SendEmailConfirmationAsync(string toEmail, string userName, string confirmationLink, CancellationToken ct = default);
    Task SendPasswordResetAsync(string toEmail, string userName, string resetLink, CancellationToken ct = default);
    Task SendAdminNotificationNewUserAsync(IEnumerable<string> adminEmails, string newUserName, string newUserEmail, CancellationToken ct = default);
    Task SendUserRegistrationApprovedAsync(string toEmail, string userName, CancellationToken ct = default);
    Task SendUserRegistrationDeniedAsync(string toEmail, string userName, CancellationToken ct = default);
}
