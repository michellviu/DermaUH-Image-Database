using Domain.DermaImage.Entities;

namespace Domain.DermaImage.Interfaces.Repository;

public interface IDermaImgRepository : IRepository<DermaImg>
{
    Task<DermaImg?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DermaImg>> GetByContributorIdAsync(Guid contributorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DermaImg>> GetByInstitutionIdAsync(Guid institutionId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<DermaImg> Items, int TotalCount)> GetPagedFilteredAsync(int page, int pageSize, DermaImgFilter? filter = null, CancellationToken cancellationToken = default);
    Task<string> GeneratePublicIdAsync(CancellationToken cancellationToken = default);
}
