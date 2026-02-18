using Domain.DermaImage.Entities;
using Domain.DermaImage.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DermaImage.Repositories;

public class DermaImgRepository : Repository<DermaImg>, IDermaImgRepository
{
    public DermaImgRepository(DermaImageDbContext context) : base(context) { }

    public async Task<DermaImg?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(i => i.Contributor)
            .Include(i => i.Institution)
            .FirstOrDefaultAsync(i => i.PublicId == publicId, cancellationToken);
    }

    public async Task<IEnumerable<DermaImg>> GetByContributorIdAsync(Guid contributorId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(i => i.ContributorId == contributorId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DermaImg>> GetByInstitutionIdAsync(Guid institutionId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(i => i.InstitutionId == institutionId)
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
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<string> GeneratePublicIdAsync(CancellationToken cancellationToken = default)
    {
        var count = await Context.Images
            .IgnoreQueryFilters()
            .CountAsync(cancellationToken);
        return $"DERM_{(count + 1):D7}";
    }
}
