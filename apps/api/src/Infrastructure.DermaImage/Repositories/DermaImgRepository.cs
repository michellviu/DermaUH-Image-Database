using Domain.DermaImage.Entities;
using Domain.DermaImage.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DermaImage.Repositories;

public class DermaImgRepository : Repository<DermaImg>, IDermaImgRepository
{
    public DermaImgRepository(DermaImageDbContext context) : base(context) { }

    private IQueryable<DermaImg> BuildVisibilityQuery(bool includePrivate)
    {
        var query = DbSet.AsNoTracking();
        if (!includePrivate)
        {
            query = query.Where(i => i.IsPublic);
        }

        return query;
    }

    public Task<DermaImg?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken = default)
    {
        return DbSet
            .Include(i => i.Contributor)
            .Include(i => i.Institution)
            .FirstOrDefaultAsync(i => i.PublicId == publicId, cancellationToken);
    }

    public async Task<IEnumerable<DermaImg>> GetByContributorIdAsync(Guid contributorId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(i => i.ContributorId == contributorId)
            .Include(i => i.Contributor)
            .Include(i => i.Institution)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DermaImg>> GetByInstitutionIdAsync(Guid institutionId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(i => i.InstitutionId == institutionId)
            .Include(i => i.Contributor)
            .Include(i => i.Institution)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<DermaImg> Items, int TotalCount)> GetPagedFilteredAsync(
        int page,
        int pageSize,
        DermaImgFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;

        var query = DbSet.AsQueryable();

        if (filter is not null)
        {
            if (filter.ImageTypes is { Count: > 0 })
            {
                query = query.Where(i => i.ImageType.HasValue && filter.ImageTypes.Contains(i.ImageType.Value));
            }

            if (filter.DiagnosisCategories is { Count: > 0 })
            {
                query = query.Where(i => i.DiagnosisCategory.HasValue && filter.DiagnosisCategories.Contains(i.DiagnosisCategory.Value));
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
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .Include(i => i.Contributor)
            .Include(i => i.Institution)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<int> CountByVisibilityAsync(bool includePrivate, CancellationToken cancellationToken = default)
    {
        return await BuildVisibilityQuery(includePrivate).CountAsync(cancellationToken);
    }

    public async Task<int> CountDistinctContributorsAsync(bool includePrivate, CancellationToken cancellationToken = default)
    {
        return await BuildVisibilityQuery(includePrivate)
            .Select(i => i.ContributorId)
            .Distinct()
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<(string Key, int Count)>> GetDiagnosisCategoryCountsAsync(bool includePrivate, CancellationToken cancellationToken = default)
    {
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
        var grouped = await BuildVisibilityQuery(includePrivate)
            .Where(i => i.AnatomSiteGeneral.HasValue)
            .GroupBy(i => i.AnatomSiteGeneral)
            .Select(g => new { Key = g.Key!.Value.ToString(), Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        return grouped.Select(x => (x.Key, x.Count)).ToList();
    }

    public async Task<IReadOnlyList<(int Year, int Month, int Count)>> GetMonthlyUploadCountsAsync(int recentMonths, bool includePrivate, CancellationToken cancellationToken = default)
    {
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

    public async Task<IReadOnlyList<(Guid InstitutionId, string InstitutionName, int Count)>> GetTopInstitutionsByImageCountAsync(int take, bool includePrivate, CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 20);

        var grouped = await BuildVisibilityQuery(includePrivate)
            .Where(i => i.InstitutionId.HasValue)
            .GroupBy(i => new
            {
                InstitutionId = i.InstitutionId!.Value,
                InstitutionName = i.Institution != null ? i.Institution.Name : "Sin institución"
            })
            .Select(g => new
            {
                g.Key.InstitutionId,
                g.Key.InstitutionName,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(take)
            .ToListAsync(cancellationToken);

        return grouped.Select(x => (x.InstitutionId, x.InstitutionName, x.Count)).ToList();
    }

    public async Task<string> GeneratePublicIdAsync(CancellationToken cancellationToken = default)
    {
        var count = await Context.Images
            .IgnoreQueryFilters()
            .CountAsync(cancellationToken);
        return $"DERM_{(count + 1):D7}";
    }
}
