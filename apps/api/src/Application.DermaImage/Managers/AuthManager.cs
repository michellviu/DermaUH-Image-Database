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

    public AuthManager(IUserRepository users, IJwtService jwt, IEmailService email)
    {
        _users = users;
        _jwt = jwt;
        _email = email;
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
            EmailConfirmed  = user.EmailConfirmed,
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
        user.PhoneNumber = dto.PhoneNumber;

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
}
