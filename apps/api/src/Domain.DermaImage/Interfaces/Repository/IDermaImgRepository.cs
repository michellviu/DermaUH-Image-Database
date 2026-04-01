using Domain.DermaImage.Entities;

namespace Domain.DermaImage.Interfaces.Repository;

public interface IDermaImgRepository : IRepository<DermaImg>
{
    Task<DermaImg?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DermaImg>> GetByContributorIdAsync(Guid contributorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DermaImg>> GetByInstitutionIdAsync(Guid institutionId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<DermaImg> Items, int TotalCount)> GetPagedFilteredAsync(int page, int pageSize, DermaImgFilter? filter = null, CancellationToken cancellationToken = default);
    Task<int> CountByVisibilityAsync(bool includePrivate, CancellationToken cancellationToken = default);
    Task<int> CountDistinctContributorsAsync(bool includePrivate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(string Key, int Count)>> GetDiagnosisCategoryCountsAsync(bool includePrivate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(string Key, int Count)>> GetSexCountsAsync(bool includePrivate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(string Key, int Count)>> GetAnatomicalSiteCountsAsync(bool includePrivate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(int Year, int Month, int Count)>> GetMonthlyUploadCountsAsync(int recentMonths, bool includePrivate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(Guid InstitutionId, string InstitutionName, int Count)>> GetTopInstitutionsByImageCountAsync(int take, bool includePrivate, CancellationToken cancellationToken = default);
    Task<string> GeneratePublicIdAsync(CancellationToken cancellationToken = default);
}
