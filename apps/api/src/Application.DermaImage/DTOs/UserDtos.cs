using Domain.DermaImage.Entities.Enums;

namespace Application.DermaImage.DTOs;

// ── User DTOs ──────────────────────────────────────────────────────────

public class CreateUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole? Role { get; set; }
    public Guid? InstitutionId { get; set; }
}

public class AssignRoleDto
{
    public UserRole Role { get; set; }
}
