using System.ComponentModel.DataAnnotations;

namespace Application.DermaImage.DTOs;

public class AssignInstitutionResponsibleDto
{
    [Required]
    public Guid UserId { get; set; }
}

public class InstitutionResponsibleResponseDto
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

public class CreateInstitutionJoinRequestDto
{
    [Required]
    public Guid InstitutionId { get; set; }
}

public class ReviewInstitutionJoinRequestDto
{
    public bool Approve { get; set; }

    [MaxLength(500)]
    public string? Comment { get; set; }
}

public class InstitutionJoinRequestResponseDto
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
}