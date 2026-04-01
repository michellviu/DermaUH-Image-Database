using System.ComponentModel.DataAnnotations;

namespace Application.DermaImage.DTOs;

// ── Register ───────────────────────────────────────────────────────────

public class RegisterDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

}

// ── Login ──────────────────────────────────────────────────────────────

public class LoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

// ── Login Response ─────────────────────────────────────────────────────

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = new List<string>();
    public bool EmailConfirmed { get; set; }
}

// ── Google Login ───────────────────────────────────────────────────────

public class GoogleLoginDto
{
    /// <summary>ID token returned by Google Identity / One Tap.</summary>
    [Required]
    public string IdToken { get; set; } = string.Empty;
}

// ── Email Confirmation ─────────────────────────────────────────────────

public class ConfirmEmailDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;
}

// ── Forgot / Reset Password ────────────────────────────────────────────

public class ForgotPasswordDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}

// ── User Profile ───────────────────────────────────────────────────────

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool EmailConfirmed { get; set; }
    public Guid? InstitutionId { get; set; }
    public string? InstitutionName { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public DateTime CreatedAt { get; set; }
}

public class UpdateProfileDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }

    public Guid? InstitutionId { get; set; }
}

public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}
