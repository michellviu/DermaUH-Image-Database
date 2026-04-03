using Domain.DermaImage.Entities;
using Domain.DermaImage.Entities.Enums;

namespace Domain.DermaImage.Interfaces.Services;

public interface IInstitutionMembershipRequestService
{
    Task<InstitutionMembershipRequest?> GetPendingAsync(Guid applicantUserId, Guid institutionId, CancellationToken cancellationToken = default);
    Task<InstitutionMembershipRequest> CreateAsync(InstitutionMembershipRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<InstitutionMembershipRequest>> GetByApplicantAsync(Guid applicantUserId, CancellationToken cancellationToken = default);
    Task<IEnumerable<InstitutionMembershipRequest>> GetPendingForInstitutionAsync(Guid institutionId, CancellationToken cancellationToken = default);
    Task<InstitutionMembershipRequest?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(InstitutionMembershipRequest request, CancellationToken cancellationToken = default);
    Task<bool> HasAnyReviewedAsync(Guid applicantUserId, Guid institutionId, InstitutionMembershipRequestStatus status, CancellationToken cancellationToken = default);
}
