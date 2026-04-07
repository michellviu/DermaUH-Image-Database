namespace Web.DermaImage.Shared.Models;

public class InstitutionResponsibleDto
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}

public class CreateInstitutionJoinRequest
{
    public Guid InstitutionId { get; set; }
}

public class ReviewInstitutionJoinRequest
{
    public bool Approve { get; set; }
    public string? Comment { get; set; }
}

public class InstitutionJoinRequestDto
{
    public Guid Id { get; set; }
    public Guid InstitutionId { get; set; }
    public string InstitutionName { get; set; } = string.Empty;

    public Guid ApplicantUserId { get; set; }
    public string ApplicantFirstName { get; set; } = string.Empty;
    public string ApplicantLastName { get; set; } = string.Empty;
    public string ApplicantEmail { get; set; } = string.Empty;
    public string ApplicantPhoneNumber { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
    public string? ReviewComment { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public string? ReviewedByFullName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public string ApplicantFullName => $"{ApplicantFirstName} {ApplicantLastName}".Trim();
    public bool IsPending => string.Equals(Status, "Pending", StringComparison.OrdinalIgnoreCase);
}