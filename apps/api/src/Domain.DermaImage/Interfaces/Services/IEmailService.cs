namespace Domain.DermaImage.Interfaces.Services;

/// <summary>
/// Abstraction for sending transactional emails.
/// </summary>
public interface IEmailService
{
    Task SendEmailConfirmationAsync(string toEmail, string userName, string confirmationLink, CancellationToken ct = default);
    Task SendPasswordResetAsync(string toEmail, string userName, string resetLink, CancellationToken ct = default);
    Task SendInstitutionJoinRequestReviewedAsync(string toEmail, string userName, string institutionName, bool approved, string? comment, CancellationToken ct = default);
}
