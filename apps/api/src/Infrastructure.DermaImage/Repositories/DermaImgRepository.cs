using Domain.DermaImage.Entities;
using Domain.DermaImage.Entities.Enums;
using Domain.DermaImage.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.DermaImage.Repositories;

public class DermaImgRepository : Repository<DermaImg>, IDermaImgRepository
{
    public DermaImgRepository(DermaImageDbContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory) { }

    public new Task<DermaImg?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Fetching image by id: {ImageId}", id);
        return DbSet
            .Include(i => i.Institution)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    private IQueryable<DermaImg> BuildVisibilityQuery(bool includePrivate)
    {
        Logger.LogInformation("Building image visibility query. IncludePrivate: {IncludePrivate}", includePrivate);
        IQueryable<DermaImg> query = DbSet.AsNoTracking().Include(i => i.Institution);

        if (!includePrivate)
        {
            query = query.Where(i => i.IsPublic);
        }

        return query;
    }

    private static IQueryable<DermaImg> ApplyFilter(IQueryable<DermaImg> query, DermaImgFilter? filter)
    {
        if (filter is null)
        {
            return query;
        }

        if (filter.ImageTypes is { Count: > 0 })
        {
            query = query.Where(i => i.ImageType.HasValue && filter.ImageTypes.Contains(i.ImageType.Value));
        }

        if (filter.DiagnosisCategories is { Count: > 0 })
        {
            query = query.Where(i => i.DiagnosisCategory.HasValue && filter.DiagnosisCategories.Contains(i.DiagnosisCategory.Value));
        }

        if (filter.InjuryTypes is { Count: > 0 })
        {
            query = query.Where(i => i.InjuryType.HasValue && filter.InjuryTypes.Contains(i.InjuryType.Value));
        }

        if (filter.FotoTypes is { Count: > 0 })
        {
            query = query.Where(i => i.FotoType.HasValue && filter.FotoTypes.Contains(i.FotoType.Value));
        }

        if (filter.DiagnosisConfirmTypes is { Count: > 0 })
        {
            query = query.Where(i => i.DiagnosisConfirmType.HasValue && filter.DiagnosisConfirmTypes.Contains(i.DiagnosisConfirmType.Value));
        }

        if (filter.Sexes is { Count: > 0 })
        {
            query = query.Where(i => i.Sex.HasValue && filter.Sexes.Contains(i.Sex.Value));
        }

        if (filter.AnatomSites is { Count: > 0 })
        {
            query = query.Where(i => i.AnatomSiteGeneral.HasValue && filter.AnatomSites.Contains(i.AnatomSiteGeneral.Value));
        }

        if (filter.IsPublic.HasValue)
        {
            query = query.Where(i => i.IsPublic == filter.IsPublic.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.DiagnosisContains))
        {
            var diagnosis = filter.DiagnosisContains.Trim().ToLower();
            query = query.Where(i => i.Diagnosis != null && i.Diagnosis.ToLower().Contains(diagnosis));
        }

        return query;
    }

    public Task<DermaImg?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Fetching image by public id: {PublicId}", publicId);
        return DbSet
            .Include(i => i.Institution)
            .FirstOrDefaultAsync(i => i.PublicId == publicId, cancellationToken);
    }

    public async Task<IEnumerable<DermaImg>> GetByContributorIdAsync(Guid contributorId, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Fetching images by contributor id: {ContributorId}", contributorId);
        return await DbSet
            .Where(i => i.ContributorId == contributorId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }



    public async Task<IReadOnlyList<DermaImg>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids is null || ids.Count == 0)
        {
            return Array.Empty<DermaImg>();
        }

        var idList = ids.Distinct().ToList();
        Logger.LogInformation("Fetching images by ids. Count: {Count}", idList.Count);

        return await DbSet
            .Include(i => i.Institution)
            .Where(i => idList.Contains(i.Id))
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<DermaImg> Items, int TotalCount)> GetPagedFilteredAsync(
        int page,
        int pageSize,
        DermaImgFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Fetching paged filtered images. Page: {Page}, PageSize: {PageSize}, HasFilter: {HasFilter}", page, pageSize, filter is not null);
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;

        var query = ApplyFilter(DbSet.AsQueryable(), filter);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        Logger.LogInformation("Fetched paged filtered images. Returned: {Count}, TotalCount: {TotalCount}", items.Count, totalCount);
        return (items, totalCount);
    }

    public async Task<IReadOnlyList<DermaImg>> GetFilteredAsync(DermaImgFilter? filter = null, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Fetching filtered images. HasFilter: {HasFilter}", filter is not null);
        var query = ApplyFilter(DbSet.AsQueryable(), filter);

        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);

        Logger.LogInformation("Fetched filtered images. Returned: {Count}", items.Count);
        return items;
    }

    public async Task<int> CountByVisibilityAsync(bool includePrivate, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Counting images by visibility. IncludePrivate: {IncludePrivate}", includePrivate);
        return await BuildVisibilityQuery(includePrivate).CountAsync(cancellationToken);
    }

    public async Task<int> CountDistinctContributorsAsync(bool includePrivate, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Counting distinct contributors. IncludePrivate: {IncludePrivate}", includePrivate);
        return await BuildVisibilityQuery(includePrivate)
            .Select(i => i.ContributorId)
            .Distinct()
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<(string Key, int Count)>> GetDiagnosisCategoryCountsAsync(bool includePrivate, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Fetching diagnosis category counts. IncludePrivate: {IncludePrivate}", includePrivate);
        var grouped = await BuildVisibilityQuery(includePrivate)
            .Where(i => i.DiagnosisCategory.HasValue)
            .GroupBy(i => i.DiagnosisCategory)
            .Select(g => new { Key = g.Key!.Value.ToString(), Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        return grouped.Select(x => (x.Key, x.Count)).ToList();
    }

    public async Task<IReadOnlyList<(string Key, int Count)>> GetSexCountsAsync(bool includePrivate, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Fetching sex counts. IncludePrivate: {IncludePrivate}", includePrivate);
        var grouped = await BuildVisibilityQuery(includePrivate)
            .Where(i => i.Sex.HasValue)
            .GroupBy(i => i.Sex)
            .Select(g => new { Key = g.Key!.Value.ToString(), Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        return grouped.Select(x => (x.Key, x.Count)).ToList();
    }

    public async Task<IReadOnlyList<(string Key, int Count)>> GetAnatomicalSiteCountsAsync(bool includePrivate, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Fetching anatomical site counts. IncludePrivate: {IncludePrivate}", includePrivate);
        var grouped = await BuildVisibilityQuery(includePrivate)
            .Where(i => i.AnatomSiteGeneral.HasValue)
            .GroupBy(i => i.AnatomSiteGeneral)
            .Select(g => new { Key = g.Key!.Value.ToString(), Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        return grouped.Select(x => (x.Key, x.Count)).ToList();
    }

    public async Task<IReadOnlyList<(string Key, int Count)>> GetPhotoTypeCountsAsync(bool includePrivate, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Fetching photo type counts. IncludePrivate: {IncludePrivate}", includePrivate);
        var grouped = await BuildVisibilityQuery(includePrivate)
            .Where(i => i.FotoType.HasValue)
            .GroupBy(i => i.FotoType)
            .Select(g => new { Key = g.Key!.Value.ToString(), Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        return grouped.Select(x => (x.Key, x.Count)).ToList();
    }

    public async Task<IReadOnlyList<(string Key, int Count)>> GetInjuryTypeCountsAsync(bool includePrivate, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Fetching injury type counts. IncludePrivate: {IncludePrivate}", includePrivate);
        var grouped = await BuildVisibilityQuery(includePrivate)
            .Where(i => i.InjuryType.HasValue)
            .GroupBy(i => i.InjuryType)
            .Select(g => new { Key = g.Key!.Value.ToString(), Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        return grouped.Select(x => (x.Key, x.Count)).ToList();
    }

    public async Task<IReadOnlyList<(int Year, int Month, int Count)>> GetMonthlyUploadCountsAsync(int recentMonths, bool includePrivate, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Fetching monthly upload counts. RecentMonths: {RecentMonths}, IncludePrivate: {IncludePrivate}", recentMonths, includePrivate);
        recentMonths = Math.Clamp(recentMonths, 1, 24);
        var utcNow = DateTime.UtcNow;
        var firstDay = new DateTime(utcNow.Year, utcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddMonths(-(recentMonths - 1));

        var grouped = await BuildVisibilityQuery(includePrivate)
            .Where(i => i.CreatedAt >= firstDay)
            .GroupBy(i => new { i.CreatedAt.Year, i.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync(cancellationToken);

        return grouped.Select(x => (x.Year, x.Month, x.Count)).ToList();
    }


    public async Task<string> GeneratePublicIdAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Generating public id for image");
        var count = await Context.Images
            .IgnoreQueryFilters()
            .CountAsync(cancellationToken);
        return $"DERM_{(count + 1):D7}";
    }

    // ── New statistical queries ────────────────────────────────────────

    public async Task<IReadOnlyList<(string Key, int Count)>> GetSkinColorCountsAsync(bool includePrivate, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Fetching skin color counts. IncludePrivate: {IncludePrivate}", includePrivate);
        var grouped = await BuildVisibilityQuery(includePrivate)
            .Where(i => i.SkinColor.HasValue)
            .GroupBy(i => i.SkinColor)
            .Select(g => new { Key = g.Key!.Value.ToString(), Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        return grouped.Select(x => (x.Key, x.Count)).ToList();
    }

    public async Task<IReadOnlyList<(string Key, int Count)>> GetDiagnosisConfirmCountsAsync(bool includePrivate, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Fetching diagnosis confirm type counts. IncludePrivate: {IncludePrivate}", includePrivate);
        var grouped = await BuildVisibilityQuery(includePrivate)
            .Where(i => i.DiagnosisConfirmType.HasValue)
            .GroupBy(i => i.DiagnosisConfirmType)
            .Select(g => new { Key = g.Key!.Value.ToString(), Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        return grouped.Select(x => (x.Key, x.Count)).ToList();
    }

    public async Task<IReadOnlyList<(string Key, int Count)>> GetImageTypeCountsAsync(bool includePrivate, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Fetching image type counts. IncludePrivate: {IncludePrivate}", includePrivate);
        var grouped = await BuildVisibilityQuery(includePrivate)
            .Where(i => i.ImageType.HasValue)
            .GroupBy(i => i.ImageType)
            .Select(g => new { Key = g.Key!.Value.ToString(), Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        return grouped.Select(x => (x.Key, x.Count)).ToList();
    }

    public async Task<IReadOnlyList<int>> GetAgeValuesAsync(bool includePrivate, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Fetching age values for statistics. IncludePrivate: {IncludePrivate}", includePrivate);
        return await BuildVisibilityQuery(includePrivate)
            .Where(i => i.AgeApprox.HasValue && i.AgeApprox.Value > 0 && i.AgeApprox.Value < 120)
            .Select(i => i.AgeApprox!.Value)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<(string RowKey, string ColKey, int Count)>> GetCrossTabAsync(
        string rowField, string colField, bool includePrivate, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Fetching cross-tab: {RowField} × {ColField}. IncludePrivate: {IncludePrivate}", rowField, colField, includePrivate);

        var query = BuildVisibilityQuery(includePrivate);

        // We build this dynamically based on field names
        var items = await query.ToListAsync(cancellationToken);

        var results = items
            .Select(i => new
            {
                Row = GetFieldValue(i, rowField),
                Col = GetFieldValue(i, colField)
            })
            .Where(x => x.Row != null && x.Col != null)
            .GroupBy(x => new { x.Row, x.Col })
            .Select(g => (RowKey: g.Key.Row!, ColKey: g.Key.Col!, Count: g.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();

        return results;
    }

    public async Task<IReadOnlyList<(string SiteKey, int MelanomaCount, int TotalCount)>> GetMelanomaByAnatomicalSiteAsync(
        bool includePrivate, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Fetching melanoma by anatomical site. IncludePrivate: {IncludePrivate}", includePrivate);

        var query = BuildVisibilityQuery(includePrivate)
            .Where(i => i.AnatomSiteGeneral.HasValue);

        var grouped = await query
            .GroupBy(i => i.AnatomSiteGeneral)
            .Select(g => new
            {
                Site = g.Key!.Value.ToString(),
                MelanomaCount = g.Count(i => i.InjuryType == InjuryType.Melanoma),
                TotalCount = g.Count()
            })
            .ToListAsync(cancellationToken);

        return grouped.Select(x => (x.Site, x.MelanomaCount, x.TotalCount)).ToList();
    }

    public async Task<IReadOnlyList<(string Key, int Count)>> GetProvinceCountsAsync(bool includePrivate, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Fetching province counts. IncludePrivate: {IncludePrivate}", includePrivate);
        var grouped = await BuildVisibilityQuery(includePrivate)
            .Where(i => i.Provincia.HasValue)
            .GroupBy(i => i.Provincia)
            .Select(g => new { Key = g.Key!.Value.ToString(), Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        return grouped.Select(x => (x.Key, x.Count)).ToList();
    }

    public async Task<IReadOnlyList<(string FieldName, int FilledCount, int TotalCount)>> GetDataCompletenessAsync(
        bool includePrivate, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Fetching data completeness. IncludePrivate: {IncludePrivate}", includePrivate);

        var items = await BuildVisibilityQuery(includePrivate).ToListAsync(cancellationToken);
        var total = items.Count;
        if (total == 0) return [];

        var fields = new List<(string FieldName, int FilledCount, int TotalCount)>
        {
            ("AgeApprox", items.Count(i => i.AgeApprox.HasValue), total),
            ("Sex", items.Count(i => i.Sex.HasValue), total),
            ("SkinColor", items.Count(i => i.SkinColor.HasValue), total),
            ("FotoType", items.Count(i => i.FotoType.HasValue), total),
            ("AnatomSiteGeneral", items.Count(i => i.AnatomSiteGeneral.HasValue), total),
            ("DiagnosisCategory", items.Count(i => i.DiagnosisCategory.HasValue), total),
            ("InjuryType", items.Count(i => i.InjuryType.HasValue), total),
            ("DiagnosisConfirmType", items.Count(i => i.DiagnosisConfirmType.HasValue), total),
            ("ImageType", items.Count(i => i.ImageType.HasValue), total),
            ("Diagnosis", items.Count(i => !string.IsNullOrWhiteSpace(i.Diagnosis)), total),
            ("ClinSizeLongDiamMm", items.Count(i => i.ClinSizeLongDiamMm.HasValue), total),
            ("PersonalHxMm", items.Count(i => i.PersonalHxMm.HasValue), total),
            ("FamilyHxMm", items.Count(i => i.FamilyHxMm.HasValue), total),
            ("SunExposure", items.Count(i => i.SunExposure.HasValue), total),
            ("Provincia", items.Count(i => i.Provincia.HasValue), total),
        };

        return fields;
    }

    private static string? GetFieldValue(DermaImg img, string field) => field switch
    {
        "DiagnosisCategory" => img.DiagnosisCategory?.ToString(),
        "Sex" => img.Sex?.ToString(),
        "AnatomSiteGeneral" => img.AnatomSiteGeneral?.ToString(),
        "InjuryType" => img.InjuryType?.ToString(),
        "SkinColor" => img.SkinColor?.ToString(),
        "FotoType" => img.FotoType?.ToString(),
        "PhotoType" => img.FotoType?.ToString(),
        "AgeGroup" => img.AgeApprox.HasValue ? GetAgeGroup(img.AgeApprox.Value) : null,
        _ => null
    };

    private static string GetAgeGroup(int age) => age switch
    {
        < 18 => "0-17",
        < 30 => "18-29",
        < 40 => "30-39",
        < 50 => "40-49",
        < 60 => "50-59",
        < 70 => "60-69",
        < 80 => "70-79",
        _ => "80+"
    };
}
