using Domain.DermaImage.Entities;
using Domain.DermaImage.Entities.Enums;
using Domain.DermaImage.Interfaces.Repository;
using Domain.DermaImage.Interfaces.Services;

namespace Application.DermaImage.Services;

public class InstitutionMembershipRequestService : IInstitutionMembershipRequestService
{
    private readonly IInstitutionMembershipRequestRepository _repository;

    public InstitutionMembershipRequestService(IInstitutionMembershipRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<InstitutionMembershipRequest?> GetPendingAsync(Guid applicantUserId, Guid institutionId, CancellationToken cancellationToken = default)
        => await _repository.GetPendingAsync(applicantUserId, institutionId, cancellationToken);

    public async Task<InstitutionMembershipRequest> CreateAsync(InstitutionMembershipRequest request, CancellationToken cancellationToken = default)
        => await _repository.AddAsync(request, cancellationToken);

    public async Task<IEnumerable<InstitutionMembershipRequest>> GetByApplicantAsync(Guid applicantUserId, CancellationToken cancellationToken = default)
        => await _repository.GetByApplicantAsync(applicantUserId, cancellationToken);

    public async Task<IEnumerable<InstitutionMembershipRequest>> GetPendingForInstitutionAsync(Guid institutionId, CancellationToken cancellationToken = default)
        => await _repository.GetPendingForInstitutionAsync(institutionId, cancellationToken);

    public async Task<InstitutionMembershipRequest?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _repository.GetByIdWithDetailsAsync(id, cancellationToken);

    public async Task UpdateAsync(InstitutionMembershipRequest request, CancellationToken cancellationToken = default)
        => await _repository.UpdateAsync(request, cancellationToken);

    public async Task<bool> HasAnyReviewedAsync(Guid applicantUserId, Guid institutionId, InstitutionMembershipRequestStatus status, CancellationToken cancellationToken = default)
        => await _repository.HasAnyResponsibleReviewedAsync(applicantUserId, institutionId, status, cancellationToken);
}
