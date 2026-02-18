using Microsoft.AspNetCore.Identity;

namespace Domain.DermaImage.Entities;

/// <summary>
/// Represents a user/contributor in the system.
/// Inherits from IdentityUser to leverage ASP.NET Identity for authentication and authorization.
/// </summary>
public class User : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Audit fields (previously from BaseEntity)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Institution relationship
    public Guid? InstitutionId { get; set; }
    public Institution? Institution { get; set; }

    // Navigation
    public ICollection<DermaImg> ContributedImages { get; set; } = new List<DermaImg>();

    public string FullName => $"{FirstName} {LastName}";
}
