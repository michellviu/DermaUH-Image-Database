using Domain.DermaImage.Entities;

namespace Domain.DermaImage.Interfaces.Repository;

public interface IDermaImgRepository : IRepository<DermaImg>
{
    Task<DermaImg?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DermaImg>> GetByContributorIdAsync(Guid contributorId, CancellationToken cancellationToken = default);

    Task<(IEnumerable<DermaImg> Items, int TotalCount)> GetPagedFilteredAsync(int page, int pageSize, DermaImgFilter? filter = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DermaImg>> GetFilteredAsync(DermaImgFilter? filter = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DermaImg>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
    Task<int> CountByVisibilityAsync(bool includePrivate, CancellationToken cancellationToken = default);
    Task<int> CountDistinctContributorsAsync(bool includePrivate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(string Key, int Count)>> GetDiagnosisCategoryCountsAsync(bool includePrivate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(string Key, int Count)>> GetSexCountsAsync(bool includePrivate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(string Key, int Count)>> GetAnatomicalSiteCountsAsync(bool includePrivate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(string Key, int Count)>> GetPhotoTypeCountsAsync(bool includePrivate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(string Key, int Count)>> GetInjuryTypeCountsAsync(bool includePrivate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(int Year, int Month, int Count)>> GetMonthlyUploadCountsAsync(int recentMonths, bool includePrivate, CancellationToken cancellationToken = default);
    Task<string> GeneratePublicIdAsync(CancellationToken cancellationToken = default);

    // ── New statistical queries ────────────────────────────────────────
    Task<IReadOnlyList<(string Key, int Count)>> GetSkinColorCountsAsync(bool includePrivate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(string Key, int Count)>> GetDiagnosisConfirmCountsAsync(bool includePrivate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(string Key, int Count)>> GetImageTypeCountsAsync(bool includePrivate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetAgeValuesAsync(bool includePrivate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(string RowKey, string ColKey, int Count)>> GetCrossTabAsync(string rowField, string colField, bool includePrivate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(string SiteKey, int MelanomaCount, int TotalCount)>> GetMelanomaByAnatomicalSiteAsync(bool includePrivate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(string Key, int Count)>> GetProvinceCountsAsync(bool includePrivate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(string FieldName, int FilledCount, int TotalCount)>> GetDataCompletenessAsync(bool includePrivate, CancellationToken cancellationToken = default);
}
