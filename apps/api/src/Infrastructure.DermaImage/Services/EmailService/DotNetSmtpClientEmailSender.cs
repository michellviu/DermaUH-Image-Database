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

    private const string AdminNotificationNewUserTemplateResource =
        "Infrastructure.DermaImage.Services.EmailTemplates.AdminNotificationNewUser.html";

    private const string UserRegistrationApprovedTemplateResource =
        "Infrastructure.DermaImage.Services.EmailTemplates.UserRegistrationApproved.html";

    private const string UserRegistrationDeniedTemplateResource =
        "Infrastructure.DermaImage.Services.EmailTemplates.UserRegistrationDenied.html";

    private const string AdminNotificationNewDownloadRequestTemplateResource =
        "Infrastructure.DermaImage.Services.EmailTemplates.AdminNotificationNewDownloadRequest.html";

    private const string DownloadRequestApprovedTemplateResource =
        "Infrastructure.DermaImage.Services.EmailTemplates.DownloadRequestApproved.html";

    private const string DownloadRequestDeniedTemplateResource =
        "Infrastructure.DermaImage.Services.EmailTemplates.DownloadRequestDenied.html";

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

    public async Task SendAdminNotificationNewUserAsync(IEnumerable<string> adminEmails, string newUserName, string newUserEmail, CancellationToken ct = default)
    {
        var body = await BuildTemplateBodyAsync(
            AdminNotificationNewUserTemplateResource,
            new Dictionary<string, string>
            {
                ["NewUserName"] = WebUtility.HtmlEncode(newUserName),
                ["NewUserEmail"] = WebUtility.HtmlEncode(newUserEmail),
            });

        await SendEmailAsync(
            receivers: adminEmails.ToList(),
            carbonCopy: null,
            subject: "Nueva solicitud de registro - DermaUH",
            messageBody: body,
            ct: ct);
    }

    public async Task SendUserRegistrationApprovedAsync(string toEmail, string userName, CancellationToken ct = default)
    {
        var body = await BuildTemplateBodyAsync(
            UserRegistrationApprovedTemplateResource,
            new Dictionary<string, string>
            {
                ["UserName"] = WebUtility.HtmlEncode(userName),
            });

        await SendEmailAsync(
            receivers: [toEmail],
            carbonCopy: null,
            subject: "Registro aprobado - DermaUH",
            messageBody: body,
            ct: ct);
    }

    public async Task SendUserRegistrationDeniedAsync(string toEmail, string userName, CancellationToken ct = default)
    {
        var body = await BuildTemplateBodyAsync(
            UserRegistrationDeniedTemplateResource,
            new Dictionary<string, string>
            {
                ["UserName"] = WebUtility.HtmlEncode(userName),
            });

        await SendEmailAsync(
            receivers: [toEmail],
            carbonCopy: null,
            subject: "Registro denegado - DermaUH",
            messageBody: body,
            ct: ct);
    }

    public async Task SendAdminNotificationNewDownloadRequestAsync(IEnumerable<string> adminEmails, string userName, CancellationToken ct = default)
    {
        var body = await BuildTemplateBodyAsync(
            AdminNotificationNewDownloadRequestTemplateResource,
            new Dictionary<string, string>
            {
                ["UserName"] = WebUtility.HtmlEncode(userName),
            });

        await SendEmailAsync(
            receivers: adminEmails.ToList(),
            carbonCopy: null,
            subject: "Nueva solicitud de descarga - DermaUH",
            messageBody: body,
            ct: ct);
    }

    public async Task SendDownloadRequestApprovedAsync(string toEmail, string userName, string actionUrl, CancellationToken ct = default)
    {
        var body = await BuildTemplateBodyAsync(
            DownloadRequestApprovedTemplateResource,
            new Dictionary<string, string>
            {
                ["UserName"] = WebUtility.HtmlEncode(userName),
                ["ActionUrl"] = actionUrl,
            });

        await SendEmailAsync(
            receivers: [toEmail],
            carbonCopy: null,
            subject: "Autorización de descarga aprobada - DermaUH",
            messageBody: body,
            ct: ct);
    }

    public async Task SendDownloadRequestDeniedAsync(string toEmail, string userName, CancellationToken ct = default)
    {
        var body = await BuildTemplateBodyAsync(
            DownloadRequestDeniedTemplateResource,
            new Dictionary<string, string>
            {
                ["UserName"] = WebUtility.HtmlEncode(userName),
            });

        await SendEmailAsync(
            receivers: [toEmail],
            carbonCopy: null,
            subject: "Autorización de descarga denegada - DermaUH",
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
