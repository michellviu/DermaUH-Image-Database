using Microsoft.AspNetCore.Identity;

namespace Domain.DermaImage.Entities;

public class User : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Institution relationship
    public Guid? InstitutionId { get; set; }
    public Institution? Institution { get; set; }

    // Navigation
    public ICollection<DermaImg> ContributedImages { get; set; } = new List<DermaImg>();
    public ICollection<InstitutionResponsible> ResponsibleInstitutions { get; set; } = new List<InstitutionResponsible>();
    public ICollection<InstitutionJoinRequest> InstitutionJoinRequests { get; set; } = new List<InstitutionJoinRequest>();
    public ICollection<InstitutionJoinRequest> ReviewedInstitutionJoinRequests { get; set; } = new List<InstitutionJoinRequest>();

    public string FullName => $"{FirstName} {LastName}";
}
