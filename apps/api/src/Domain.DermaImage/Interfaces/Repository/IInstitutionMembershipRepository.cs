using Domain.DermaImage.Entities;

namespace Domain.DermaImage.Interfaces.Repository;

public interface IInstitutionMembershipRepository
{
    Task<bool> IsInstitutionResponsibleAsync(Guid institutionId, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InstitutionResponsible>> GetInstitutionResponsiblesAsync(Guid institutionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InstitutionResponsible>> GetResponsibilitiesByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddInstitutionResponsibleAsync(InstitutionResponsible assignment, CancellationToken cancellationToken = default);
    Task RemoveInstitutionResponsibleAsync(Guid institutionId, Guid userId, CancellationToken cancellationToken = default);

    Task<InstitutionJoinRequest?> GetJoinRequestByIdAsync(Guid requestId, CancellationToken cancellationToken = default);
    Task<InstitutionJoinRequest?> GetPendingJoinRequestAsync(Guid applicantUserId, Guid institutionId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<InstitutionJoinRequest> Items, int TotalCount)> GetJoinRequestsByUserAsync(Guid applicantUserId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<InstitutionJoinRequest> Items, int TotalCount)> GetInboxJoinRequestsAsync(IEnumerable<Guid> institutionIds, int page, int pageSize, bool includeReviewed = false, CancellationToken cancellationToken = default);
    Task AddJoinRequestAsync(InstitutionJoinRequest joinRequest, CancellationToken cancellationToken = default);
    Task UpdateJoinRequestAsync(InstitutionJoinRequest joinRequest, CancellationToken cancellationToken = default);
}