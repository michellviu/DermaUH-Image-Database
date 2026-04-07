using Domain.DermaImage.Entities.Enums;

namespace Domain.DermaImage.Entities;

public class InstitutionJoinRequest : BaseEntity
{
    public Guid InstitutionId { get; set; }
    public Institution? Institution { get; set; }

    public Guid ApplicantUserId { get; set; }
    public User? ApplicantUser { get; set; }

    public InstitutionJoinRequestStatus Status { get; set; } = InstitutionJoinRequestStatus.Pending;

    public Guid? ReviewedByUserId { get; set; }
    public User? ReviewedByUser { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewComment { get; set; }
}