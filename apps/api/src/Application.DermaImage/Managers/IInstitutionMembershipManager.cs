using Application.DermaImage.DTOs;

namespace Application.DermaImage.Managers;

public interface IInstitutionMembershipManager
{
    Task<IReadOnlyList<InstitutionResponsibleResponseDto>> GetInstitutionResponsiblesAsync(Guid institutionId, CancellationToken cancellationToken = default);
    Task AddResponsibleAsync(Guid institutionId, Guid userId, Guid actorUserId, CancellationToken cancellationToken = default);
    Task RemoveResponsibleAsync(Guid institutionId, Guid userId, CancellationToken cancellationToken = default);

    Task<InstitutionJoinRequestResponseDto> CreateJoinRequestAsync(Guid requesterUserId, CreateInstitutionJoinRequestDto dto, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<InstitutionJoinRequestResponseDto> Items, int TotalCount)> GetMyJoinRequestsAsync(Guid requesterUserId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task LeaveInstitutionAsync(Guid requesterUserId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<InstitutionJoinRequestResponseDto> Items, int TotalCount)> GetResponsibleInboxAsync(Guid responsibleUserId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<InstitutionJoinRequestResponseDto> ReviewJoinRequestAsync(Guid responsibleUserId, Guid requestId, ReviewInstitutionJoinRequestDto dto, CancellationToken cancellationToken = default);
}