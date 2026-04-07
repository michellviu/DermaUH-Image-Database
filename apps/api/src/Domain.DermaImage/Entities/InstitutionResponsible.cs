namespace Domain.DermaImage.Entities;

public class InstitutionResponsible : BaseEntity
{
    public Guid InstitutionId { get; set; }
    public Institution? Institution { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public Guid? AssignedByUserId { get; set; }
}