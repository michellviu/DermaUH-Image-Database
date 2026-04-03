using Application.DermaImage.DTOs;
using Domain.DermaImage.Entities;
using Domain.DermaImage.Entities.Enums;
using Domain.DermaImage.Interfaces.Repository;
using Domain.DermaImage.Interfaces.Services;
using Microsoft.AspNetCore.Identity;

namespace Application.DermaImage.Managers;

public class AuthManager : IAuthManager
{
    private readonly IUserRepository _users;
    private readonly IJwtService _jwt;
    private readonly IEmailService _email;
    private readonly IInstitutionService _institutions;
    private readonly IInstitutionMembershipRequestService _membershipRequests;

    public AuthManager(
        IUserRepository users,
        IJwtService jwt,
        IEmailService email,
        IInstitutionService institutions,
        IInstitutionMembershipRequestService membershipRequests)
    {
        _users = users;
        _jwt = jwt;
        _email = email;
        _institutions = institutions;
        _membershipRequests = membershipRequests;
    }

    // ── Register ────────────────────────────────────────────────────────

    public async Task<(bool Success, string? Error)> RegisterAsync(
        RegisterDto dto, string confirmationBaseUrl, CancellationToken ct = default)
    {
        // Check duplicate email
        var existing = await _users.GetByEmailAsync(dto.Email, ct);
        if (existing is not null)
            return (false, "Ya existe una cuenta con ese correo electrónico.");

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName  = dto.LastName,
            Email     = dto.Email,
            UserName  = BuildUserName(dto.Email),
            EmailConfirmed = false,
        };

        User created;
        try
        {
            created = await _users.CreateAsync(user, dto.Password, ct);
            await _users.AddToRoleAsync(created, nameof(UserRole.Viewer));
        }
        catch (InvalidOperationException ex)
        {
            return (false, SimplifyRepositoryError(ex.Message));
        }

        // Send confirmation email
        var token = await _users.GenerateEmailConfirmationTokenAsync(created);
        var encodedToken = Uri.EscapeDataString(token);
        var link = $"{confirmationBaseUrl}?userId={created.Id}&token={encodedToken}";
        await _email.SendEmailConfirmationAsync(created.Email!, created.FirstName, link, ct);

        return (true, null);
    }

    // ── Login ───────────────────────────────────────────────────────────

    public async Task<(LoginResponseDto? Response, string? Error)> LoginAsync(
        LoginDto dto, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(dto.Email, ct);
        if (user is null)
            return (null, "Credenciales incorrectas.");

        if (!await _users.CheckPasswordAsync(user, dto.Password))
            return (null, "Credenciales incorrectas.");

        if (!user.EmailConfirmed)
            return (null, "Debes confirmar tu correo electrónico antes de iniciar sesión.");

        if (!user.IsActive)
            return (null, "Tu cuenta ha sido desactivada. Contacta al administrador.");

        var roles = await _users.GetRolesAsync(user);
        var token = _jwt.GenerateToken(user, roles);

        return (new LoginResponseDto
        {
            Token          = token,
            UserId         = user.Id,
            Email          = user.Email!,
            FullName       = user.FullName,
            Roles          = roles,
            EmailConfirmed = user.EmailConfirmed,
        }, null);
    }

    // ── Google Login ────────────────────────────────────────────────────

    public async Task<(LoginResponseDto? Response, string? Error)> GoogleLoginAsync(
        GoogleLoginDto dto, CancellationToken ct = default)
    {
        // Validate Google ID token
        Google.Apis.Auth.GoogleJsonWebSignature.Payload? payload;
        try
        {
            payload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(dto.IdToken);
        }
        catch
        {
            return (null, "Token de Google inválido.");
        }

        // Find existing user or create a new one
        var user = await _users.FindByLoginAsync("Google", payload.Subject)
                ?? await _users.GetByEmailAsync(payload.Email, ct);

        if (user is null)
        {
            user = new User
            {
                FirstName      = payload.GivenName ?? string.Empty,
                LastName       = payload.FamilyName ?? string.Empty,
                Email          = payload.Email,
                UserName       = BuildUserName(payload.Email),
                EmailConfirmed = true,
                IsActive       = true,
            };
            user = await _users.CreateExternalAsync(user, ct);
            await _users.AddToRoleAsync(user, nameof(UserRole.Viewer));
        }

        // Ensure Google login is linked
        var logins = await _users.FindByLoginAsync("Google", payload.Subject);
        if (logins is null)
            await _users.AddLoginAsync(user, new UserLoginInfo("Google", payload.Subject, "Google"));

        if (!user.IsActive)
            return (null, "Tu cuenta ha sido desactivada.");

        var roles = await _users.GetRolesAsync(user);
        var token = _jwt.GenerateToken(user, roles);

        return (new LoginResponseDto
        {
            Token          = token,
            UserId         = user.Id,
            Email          = user.Email!,
            FullName       = user.FullName,
            Roles          = roles,
            EmailConfirmed = user.EmailConfirmed,
        }, null);
    }

    // ── Confirm Email ───────────────────────────────────────────────────

    public async Task<(bool Success, string? Error)> ConfirmEmailAsync(
        ConfirmEmailDto dto, CancellationToken ct = default)
    {
        if (!Guid.TryParse(dto.UserId, out var guid))
            return (false, "Id de usuario inválido.");

        var user = await _users.GetByIdAsync(guid, ct);
        if (user is null)
            return (false, "Usuario no encontrado.");

        var result = await _users.ConfirmEmailAsync(user, dto.Token);
        if (!result.Succeeded)
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

        return (true, null);
    }

    // ── Forgot Password ─────────────────────────────────────────────────

    public async Task<(bool Success, string? Error)> ForgotPasswordAsync(
        ForgotPasswordDto dto, string resetBaseUrl, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(dto.Email, ct);
        // Always return success (don't reveal whether email exists)
        if (user is null || !user.EmailConfirmed)
            return (true, null);

        var token = await _users.GeneratePasswordResetTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);
        var link = $"{resetBaseUrl}?userId={user.Id}&token={encodedToken}";
        await _email.SendPasswordResetAsync(user.Email!, user.FirstName, link, ct);

        return (true, null);
    }

    // ── Reset Password ──────────────────────────────────────────────────

    public async Task<(bool Success, string? Error)> ResetPasswordAsync(
        ResetPasswordDto dto, CancellationToken ct = default)
    {
        if (!Guid.TryParse(dto.UserId, out var guid))
            return (false, "Id de usuario inválido.");

        var user = await _users.GetByIdAsync(guid, ct);
        if (user is null)
            return (false, "Usuario no encontrado.");

        var result = await _users.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

        return (true, null);
    }

    // ── Profile ─────────────────────────────────────────────────────────

    public async Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct);
        if (user is null) return null;

        var roles = await _users.GetRolesAsync(user);
        return new UserProfileDto
        {
            Id              = user.Id,
            FirstName       = user.FirstName,
            LastName        = user.LastName,
            Email           = user.Email ?? string.Empty,
            PhoneNumber     = user.PhoneNumber,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            EmailConfirmed  = user.EmailConfirmed,
            IsInstitutionResponsible = user.IsInstitutionResponsible,
            ResponsibleInstitutionId = user.ResponsibleInstitutionId,
            ResponsibleInstitutionName = user.ResponsibleInstitution?.Name,
            InstitutionId   = user.InstitutionId,
            InstitutionName = user.Institution?.Name,
            Roles           = roles,
            CreatedAt       = user.CreatedAt,
        };
    }

    public async Task<(bool Success, string? Error)> UpdateProfileAsync(
        Guid userId, UpdateProfileDto dto, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct);
        if (user is null)
            return (false, "Usuario no encontrado.");

        user.FirstName     = dto.FirstName;
        user.LastName      = dto.LastName;
        user.PhoneNumber   = dto.PhoneNumber;
        user.InstitutionId = dto.InstitutionId;

        await _users.UpdateAsync(user, ct);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ConfirmPhoneAsync(
        Guid userId, ConfirmPhoneDto dto, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct);
        if (user is null)
            return (false, "Usuario no encontrado.");

        if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
            return (false, "El número de teléfono es obligatorio.");

        user.PhoneNumber = dto.PhoneNumber.Trim();
        user.PhoneNumberConfirmed = true;

        await _users.UpdateAsync(user, ct);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(
        Guid userId, ChangePasswordDto dto, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct);
        if (user is null)
            return (false, "Usuario no encontrado.");

        IdentityResult result;
        if (!await _users.HasPasswordAsync(user))
        {
            // External users might not have a local password yet.
            result = await _users.AddPasswordAsync(user, dto.NewPassword);
        }
        else
        {
            result = await _users.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        }

        if (!result.Succeeded)
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

        return (true, null);
    }

    public async Task<(InstitutionMembershipRequestDto? Request, string? Error)> CreateInstitutionMembershipRequestAsync(
        Guid userId, CreateInstitutionMembershipRequestDto dto, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct);
        if (user is null)
            return (null, "Usuario no encontrado.");

        if (user.InstitutionId == dto.InstitutionId)
            return (null, "Ya perteneces a esta institución.");

        if (string.IsNullOrWhiteSpace(user.PhoneNumber))
            return (null, "Debes registrar un teléfono móvil para solicitar pertenecer a una institución.");

        if (!user.PhoneNumberConfirmed)
            return (null, "Debes confirmar tu teléfono móvil antes de solicitar pertenecer a una institución.");

        var institution = await _institutions.GetByIdAsync(dto.InstitutionId, ct);
        if (institution is null)
            return (null, "La institución seleccionada no existe.");

        var hasResponsible = (await _users.GetInstitutionResponsiblesAsync(dto.InstitutionId, ct)).Any();
        if (!hasResponsible)
            return (null, "La institución no tiene responsables asignados.");

        var pending = await _membershipRequests.GetPendingAsync(userId, dto.InstitutionId, ct);
        if (pending is not null)
            return (null, "Ya tienes una solicitud pendiente para esta institución.");

        var created = await _membershipRequests.CreateAsync(new InstitutionMembershipRequest
        {
            InstitutionId = dto.InstitutionId,
            ApplicantUserId = userId,
            Status = InstitutionMembershipRequestStatus.Pending,
        }, ct);

        var withDetails = await _membershipRequests.GetByIdWithDetailsAsync(created.Id, ct);
        return (MapMembershipRequest(withDetails ?? created), null);
    }

    public async Task<IEnumerable<InstitutionMembershipRequestDto>> GetMyInstitutionMembershipRequestsAsync(Guid userId, CancellationToken ct = default)
    {
        var requests = await _membershipRequests.GetByApplicantAsync(userId, ct);
        return requests.Select(MapMembershipRequest);
    }

    public async Task<(IEnumerable<InstitutionMembershipRequestDto>? Requests, string? Error)> GetInstitutionInboxAsync(Guid responsibleUserId, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(responsibleUserId, ct);
        if (user is null)
            return (null, "Usuario no encontrado.");

        if (!user.IsInstitutionResponsible || user.ResponsibleInstitutionId is null)
            return (null, "No tienes permisos para revisar solicitudes institucionales.");

        var requests = await _membershipRequests.GetPendingForInstitutionAsync(user.ResponsibleInstitutionId.Value, ct);
        return (requests.Select(MapMembershipRequest), null);
    }

    public async Task<(InstitutionMembershipRequestDto? Request, string? Error)> ReviewInstitutionMembershipRequestAsync(
        Guid responsibleUserId,
        Guid requestId,
        ReviewInstitutionMembershipRequestDto dto,
        CancellationToken ct = default)
    {
        var request = await _membershipRequests.GetByIdWithDetailsAsync(requestId, ct);
        if (request is null)
            return (null, "Solicitud no encontrada.");

        var isResponsible = await _users.IsInstitutionResponsibleAsync(responsibleUserId, request.InstitutionId, ct);
        if (!isResponsible)
            return (null, "No tienes permisos para revisar esta solicitud.");

        if (request.Status != InstitutionMembershipRequestStatus.Pending)
            return (null, "Esta solicitud ya fue procesada.");

        request.Status = dto.Approve
            ? InstitutionMembershipRequestStatus.Approved
            : InstitutionMembershipRequestStatus.Denied;
        request.ReviewedByUserId = responsibleUserId;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewMessage = dto.Message?.Trim();

        await _membershipRequests.UpdateAsync(request, ct);

        if (dto.Approve)
        {
            var applicant = request.ApplicantUser ?? await _users.GetByIdAsync(request.ApplicantUserId, ct);
            if (applicant is not null)
            {
                applicant.InstitutionId = request.InstitutionId;
                await _users.UpdateAsync(applicant, ct);
            }
        }

        if (!string.IsNullOrWhiteSpace(request.ApplicantUser?.Email))
        {
            await _email.SendInstitutionMembershipRequestReviewedAsync(
                request.ApplicantUser.Email!,
                request.ApplicantUser.FirstName,
                request.Institution?.Name ?? "Institución",
                dto.Approve,
                request.ReviewMessage,
                ct);
        }

        var updated = await _membershipRequests.GetByIdWithDetailsAsync(requestId, ct);
        return (MapMembershipRequest(updated ?? request), null);
    }

    private static string SimplifyRepositoryError(string message)
    {
        const string prefix = "Failed to create user:";
        return message.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? message[prefix.Length..].Trim()
            : message;
    }

    private static string BuildUserName(string email)
    {
        var localPart = email.Split('@')[0];
        var filtered = new string(localPart
            .Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.')
            .ToArray());

        if (string.IsNullOrWhiteSpace(filtered))
            filtered = "user";

        return $"{filtered}_{Guid.NewGuid():N}";
    }

    private static InstitutionMembershipRequestDto MapMembershipRequest(InstitutionMembershipRequest request)
    {
        return new InstitutionMembershipRequestDto
        {
            Id = request.Id,
            InstitutionId = request.InstitutionId,
            InstitutionName = request.Institution?.Name ?? string.Empty,
            ApplicantUserId = request.ApplicantUserId,
            ApplicantFullName = request.ApplicantUser?.FullName ?? string.Empty,
            ApplicantEmail = request.ApplicantUser?.Email ?? string.Empty,
            ApplicantPhoneNumber = request.ApplicantUser?.PhoneNumber,
            ApplicantPhoneConfirmed = request.ApplicantUser?.PhoneNumberConfirmed ?? false,
            Status = request.Status,
            CreatedAt = request.CreatedAt,
            ReviewedAt = request.ReviewedAt,
            ReviewMessage = request.ReviewMessage,
            ReviewedByUserId = request.ReviewedByUserId,
            ReviewedByName = request.ReviewedByUser?.FullName,
        };
    }
}
