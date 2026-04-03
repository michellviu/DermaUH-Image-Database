using System.ComponentModel.DataAnnotations;
using Domain.DermaImage.Entities.Enums;

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
    public bool PhoneNumberConfirmed { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool IsInstitutionResponsible { get; set; }
    public Guid? ResponsibleInstitutionId { get; set; }
    public string? ResponsibleInstitutionName { get; set; }
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

public class ConfirmPhoneDto
{
    [Required]
    public string PhoneNumber { get; set; } = string.Empty;
}

public class CreateInstitutionMembershipRequestDto
{
    [Required]
    public Guid InstitutionId { get; set; }
}

public class ReviewInstitutionMembershipRequestDto
{
    [Required]
    public bool Approve { get; set; }

    [MaxLength(1000)]
    public string? Message { get; set; }
}

public class InstitutionMembershipRequestDto
{
    public Guid Id { get; set; }
    public Guid InstitutionId { get; set; }
    public string InstitutionName { get; set; } = string.Empty;
    public Guid ApplicantUserId { get; set; }
    public string ApplicantFullName { get; set; } = string.Empty;
    public string ApplicantEmail { get; set; } = string.Empty;
    public string? ApplicantPhoneNumber { get; set; }
    public bool ApplicantPhoneConfirmed { get; set; }
    public InstitutionMembershipRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewMessage { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public string? ReviewedByName { get; set; }
}

public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}
