using Domain.DermaImage.Entities;
using Domain.DermaImage.Entities.Enums;

namespace Domain.DermaImage.Interfaces.Repository;

public interface IInstitutionMembershipRequestRepository : IRepository<InstitutionMembershipRequest>
{
    Task<InstitutionMembershipRequest?> GetPendingAsync(Guid applicantUserId, Guid institutionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<InstitutionMembershipRequest>> GetByApplicantAsync(Guid applicantUserId, CancellationToken cancellationToken = default);
    Task<IEnumerable<InstitutionMembershipRequest>> GetPendingForInstitutionAsync(Guid institutionId, CancellationToken cancellationToken = default);
    Task<InstitutionMembershipRequest?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> HasAnyResponsibleReviewedAsync(Guid applicantUserId, Guid institutionId, InstitutionMembershipRequestStatus status, CancellationToken cancellationToken = default);
}
