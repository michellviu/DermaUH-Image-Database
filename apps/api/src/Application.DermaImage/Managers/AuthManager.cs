using Application.DermaImage.DTOs;
using Domain.DermaImage.Entities;
using Domain.DermaImage.Entities.Enums;
using Domain.DermaImage.Interfaces.Repository;
using Domain.DermaImage.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Application.DermaImage.Managers;

public class AuthManager : IAuthManager
{
    private readonly IUserRepository _users;
    private readonly IJwtService _jwt;
    private readonly IEmailService _email;
    private readonly string _googleClientId;

    public AuthManager(IUserRepository users, IJwtService jwt, IEmailService email, IConfiguration config)
    {
        _users = users;
        _jwt = jwt;
        _email = email;
        _googleClientId = config["Google:ClientId"] ?? string.Empty;
    }


    public async Task<(bool Success, string? Error)> RegisterAsync(
        RegisterDto dto, string confirmationBaseUrl, CancellationToken ct = default)
    {

        var existing = await _users.GetByEmailAsync(dto.Email, ct);
        if (existing is not null)
            return (false, "Ya existe una cuenta con ese correo electrónico.");

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            UserName = BuildUserName(dto.Email),
            EmailConfirmed = false,
            IsActive = false,
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

        var token = await _users.GenerateEmailConfirmationTokenAsync(created);
        var encodedToken = Uri.EscapeDataString(token);
        var link = $"{confirmationBaseUrl}?userId={created.Id}&token={encodedToken}";
        await _email.SendEmailConfirmationAsync(created.Email!, created.FirstName, link, ct);

        var admins = await _users.GetActiveUsersByRoleAsync(UserRole.Admin, ct);
        var adminEmails = admins.Select(a => a.Email).Where(e => !string.IsNullOrWhiteSpace(e)).Cast<string>().ToList();
        if (adminEmails.Count > 0)
        {
            await _email.SendAdminNotificationNewUserAsync(adminEmails, created.FullName, created.Email!, ct);
        }

        return (true, null);
    }


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
            return (null, "Tu cuenta no está activa. Espere a que sea aprobada o contacte al administrador.");

        var roles = await _users.GetRolesAsync(user);
        var token = _jwt.GenerateToken(user, roles);

        return (new LoginResponseDto
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Roles = roles,
            EmailConfirmed = user.EmailConfirmed,
        }, null);
    }


    public async Task<(LoginResponseDto? Response, string? Error)> GoogleLoginAsync(
        GoogleLoginDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_googleClientId))
        {
            return (null, "Login con Google no está configurado.");
        }

        // Validate Google ID token
        Google.Apis.Auth.GoogleJsonWebSignature.Payload? payload;
        try
        {
            payload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(
                dto.IdToken,
                new Google.Apis.Auth.GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _googleClientId }
                });
        }
        catch
        {
            return (null, "Token de Google inválido.");
        }

        // Find existing user or create a new one
        var user = await _users.GetByEmailAsync(payload.Email, ct);

        if (user is null)
        {
            user = new User
            {
                FirstName = payload.GivenName ?? string.Empty,
                LastName = payload.FamilyName ?? string.Empty,
                Email = payload.Email,
                UserName = BuildUserName(payload.Email),
                EmailConfirmed = true,
                IsActive = false,
            };
            user = await _users.CreateExternalAsync(user, ct);
            await _users.AddToRoleAsync(user, nameof(UserRole.Viewer));
        }

        // If the user was just created, we need to notify admins and they can't login yet
        if (!user.IsActive)
        {
            var admins = await _users.GetActiveUsersByRoleAsync(UserRole.Admin, ct);
            var adminEmails = admins.Select(a => a.Email).Where(e => !string.IsNullOrWhiteSpace(e)).Cast<string>().ToList();
            if (adminEmails.Count > 0)
            {
                await _email.SendAdminNotificationNewUserAsync(adminEmails, user.FullName, user.Email!, ct);
            }
            return (null, "Tu cuenta ha sido creada y está pendiente de aprobación por un administrador.");
        }

        var roles = await _users.GetRolesAsync(user);
        var token = _jwt.GenerateToken(user, roles);

        return (new LoginResponseDto
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Roles = roles,
            EmailConfirmed = user.EmailConfirmed,
        }, null);
    }


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


    public async Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct);
        if (user is null) return null;

        var roles = await _users.GetRolesAsync(user);
        return new UserProfileDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            Roles = roles,
            CreatedAt = user.CreatedAt,
        };
    }

    public async Task<(bool Success, string? Error)> UpdateProfileAsync(
        Guid userId, UpdateProfileDto dto, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct);
        if (user is null)
            return (false, "Usuario no encontrado.");

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
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
