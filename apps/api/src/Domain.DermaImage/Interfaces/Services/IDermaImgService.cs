using Domain.DermaImage.Entities;

namespace Domain.DermaImage.Interfaces.Services;

public interface IDermaImgService
{
    Task<DermaImg?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DermaImg?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<DermaImg> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, DermaImgFilter? filter = null, CancellationToken cancellationToken = default);
    Task<DermaImg> CreateAsync(DermaImg image, CancellationToken cancellationToken = default);
    Task UpdateAsync(DermaImg image, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<DermaImg>> GetByContributorAsync(Guid contributorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DermaImg>> GetByInstitutionAsync(Guid institutionId, CancellationToken cancellationToken = default);
}
