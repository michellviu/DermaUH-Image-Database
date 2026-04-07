using Domain.DermaImage.Interfaces.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Net;
using System.Reflection;

namespace Infrastructure.DermaImage.Services.Emailing;

public class DotNetSmtpClientEmailSender : IEmailService
{
    private const string ConfirmationTemplateResource =
        "Infrastructure.DermaImage.Services.EmailTemplates.EmailConfirmation.html";

    private const string ResetPasswordTemplateResource =
        "Infrastructure.DermaImage.Services.EmailTemplates.PasswordReset.html";

    private const string InstitutionJoinRequestReviewedTemplateResource =
        "Infrastructure.DermaImage.Services.EmailTemplates.InstitutionJoinRequestReviewed.html";

    private const string InstitutionJoinRequestReviewedCommentTemplateResource =
        "Infrastructure.DermaImage.Services.EmailTemplates.InstitutionJoinRequestReviewedComment.html";

    private const string GenericFallbackTemplateResource =
        "Infrastructure.DermaImage.Services.EmailTemplates.GenericFallback.html";

    private readonly ILogger<DotNetSmtpClientEmailSender> logger;
    private readonly string fromAddress;
    private readonly string displayName;
    private readonly EmailSettings emailSettings;

    public DotNetSmtpClientEmailSender(
        IOptions<EmailSettings> emailSettings,
        ILogger<DotNetSmtpClientEmailSender> logger
    )
    {
        this.logger = logger;
        this.emailSettings = emailSettings.Value;
        fromAddress = this.emailSettings.EmailAddress;
        displayName = this.emailSettings.EmailAddressDisplay;
    }

    public async Task SendEmailConfirmationAsync(string toEmail, string userName, string confirmationLink, CancellationToken ct = default)
    {
        var body = await BuildTemplateBodyAsync(
            ConfirmationTemplateResource,
            new Dictionary<string, string>
            {
                ["UserName"] = WebUtility.HtmlEncode(userName),
                ["ActionUrl"] = confirmationLink,
            });

        await SendEmailAsync(
            receivers: [toEmail],
            carbonCopy: null,
            subject: "Confirma tu correo - DermaUH",
            messageBody: body,
            ct: ct);
    }

    public async Task SendPasswordResetAsync(string toEmail, string userName, string resetLink, CancellationToken ct = default)
    {
        var body = await BuildTemplateBodyAsync(
            ResetPasswordTemplateResource,
            new Dictionary<string, string>
            {
                ["UserName"] = WebUtility.HtmlEncode(userName),
                ["ActionUrl"] = resetLink,
            });

        await SendEmailAsync(
            receivers: [toEmail],
            carbonCopy: null,
            subject: "Restablecer contraseña - DermaUH",
            messageBody: body,
            ct: ct);
    }

    public async Task SendInstitutionJoinRequestReviewedAsync(string toEmail, string userName, string institutionName, bool approved, string? comment, CancellationToken ct = default)
    {
        var safeUserName = WebUtility.HtmlEncode(userName);
        var safeInstitutionName = WebUtility.HtmlEncode(institutionName);
        var safeComment = WebUtility.HtmlEncode(comment?.Trim() ?? string.Empty);

        var statusBadgeText = approved ? "Aprobada" : "Denegada";
        var statusBadgeBg = approved ? "#e7f8ef" : "#fdebec";
        var statusBadgeColor = approved ? "#1f7a4f" : "#9f2d3b";
        var resultMessage = approved
            ? "Tu solicitud fue aprobada. Ya puedes trabajar con esta institución en DermaUH."
            : "Tu solicitud no fue aprobada en esta ocasión. Puedes revisar tus datos y volver a intentarlo.";

            var commentSection = string.Empty;
            if (!string.IsNullOrWhiteSpace(safeComment))
            {
                commentSection = await BuildTemplateBodyAsync(
                InstitutionJoinRequestReviewedCommentTemplateResource,
                new Dictionary<string, string>
                {
                    ["CommentText"] = safeComment,
                });
            }

        var body = await BuildTemplateBodyAsync(
            InstitutionJoinRequestReviewedTemplateResource,
            new Dictionary<string, string>
            {
                ["UserName"] = safeUserName,
                ["InstitutionName"] = safeInstitutionName,
                ["StatusBadgeText"] = statusBadgeText,
                ["StatusBadgeBg"] = statusBadgeBg,
                ["StatusBadgeColor"] = statusBadgeColor,
                ["ResultMessage"] = WebUtility.HtmlEncode(resultMessage),
                ["CommentSection"] = commentSection,
            });

        await SendEmailAsync(
            receivers: [toEmail],
            carbonCopy: null,
            subject: "Actualización de solicitud institucional - DermaUH",
            messageBody: body,
            ct: ct);
    }

    private async Task SendEmailAsync(
        IReadOnlyList<string> receivers,
        IReadOnlyList<string>? carbonCopy,
        string subject,
        string messageBody,
        CancellationToken ct = default)
    {
        if (receivers.Count == 0)
            return;

        if (string.IsNullOrWhiteSpace(emailSettings.SmtpServerAddress))
        {
            logger.LogInformation(
                "[DEV EMAIL] To: {To} | Subject: {Subject} (set EmailSettings__SmtpServerAddress to enable real SMTP)",
                string.Join(",", receivers),
                subject);
            return;
        }

        try
        {
            using var message = GetMessage(receivers, carbonCopy, subject, messageBody);
            message.From.Add(new MailboxAddress(displayName, fromAddress));

            using var client = new SmtpClient();

            var options = emailSettings.EnableSSL
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTlsWhenAvailable;

            logger.LogInformation("Email server -> Trying to connect to email server ...");
            await client.ConnectAsync(emailSettings.SmtpServerAddress, emailSettings.SmtpServerPort, options, ct);

            logger.LogInformation("Email server -> connected!");

            if (client.Capabilities.HasFlag(SmtpCapabilities.Authentication))
            {
                logger.LogInformation("Email server -> requires authentication");
                if (!string.IsNullOrWhiteSpace(emailSettings.SmtpUserName))
                {
                    await client.AuthenticateAsync(emailSettings.SmtpUserName, emailSettings.SmtpPassword, ct);
                    logger.LogInformation("Email server -> authenticated");
                }
                else
                {
                    logger.LogWarning("Email server -> authentication supported but no username configured");
                }
            }
            else
                logger.LogInformation("Email server -> does not require authentication");

            logger.LogInformation("Email server -> connection established. Trying to send the email");

            var response = await client.SendAsync(message, ct);

            logger.LogInformation("Email server -> response: {response}", response);

            await client.DisconnectAsync(true, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Email server -> An error occurred while sending the email\n");
            logger.LogWarning(
                "Email delivery failed for {Receivers}. Registration flow will continue; verify SMTP settings.",
                string.Join(",", receivers));
        }
    }

    private async Task<string> BuildTemplateBodyAsync(string resourceName, IReadOnlyDictionary<string, string> replacements)
    {
        var template = await LoadTemplateAsync(resourceName);

        foreach (var replacement in replacements)
        {
            template = template.Replace(
                $"{{{{{replacement.Key}}}}}",
                replacement.Value,
                StringComparison.Ordinal);
        }

        return template;
    }

    private async Task<string> LoadTemplateAsync(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            logger.LogError("Email template resource not found: {ResourceName}", resourceName);
            stream = assembly.GetManifestResourceStream(GenericFallbackTemplateResource);
            if (stream is null)
            {
                logger.LogError("Fallback email template resource not found: {ResourceName}", GenericFallbackTemplateResource);
                return string.Empty;
            }
        }

        await using (stream)
        {
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
    }

    private MimeMessage GetMessage(
        IEnumerable<string> receivers,
        IEnumerable<string>? carbonCopy,
        string subject,
        string messageBody)
    {
        var subjectPrefix = emailSettings.EnvironmentSubjectPrefix.Trim();
        var finalSubject = string.IsNullOrWhiteSpace(subjectPrefix)
            ? subject
            : $"[{subjectPrefix}] {subject}";

        var message = new MimeMessage
        {
            Subject = finalSubject,
        };

        foreach (var item in receivers)
            message.To.Add(new MailboxAddress(item, item));

        if (carbonCopy is not null)
            foreach (var item in carbonCopy)
                message.Cc.Add(new MailboxAddress(item, item));

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = messageBody,
        };

        message.Body = bodyBuilder.ToMessageBody();

        return message;
    }
}
