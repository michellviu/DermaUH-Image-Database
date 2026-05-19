using Application.DermaImage.DTOs;

namespace Application.DermaImage.Managers;

public interface IInstitutionManager
{
    Task<IReadOnlyList<InstitutionResponseDto>> GetInstitutionsAsync(bool includePrivate, CancellationToken cancellationToken = default);
}
