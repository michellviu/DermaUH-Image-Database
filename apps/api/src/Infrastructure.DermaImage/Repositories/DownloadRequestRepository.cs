using Domain.DermaImage.Entities;
using Domain.DermaImage.Entities.Enums;
using Domain.DermaImage.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.DermaImage.Repositories;

public class DownloadRequestRepository : Repository<DownloadRequest>, IDownloadRequestRepository
{
    public DownloadRequestRepository(DermaImageDbContext context, ILoggerFactory loggerFactory)
        : base(context, loggerFactory) { }

    public async Task<(IEnumerable<DownloadRequest> Items, int TotalCount)> GetPendingPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = DbSet
            .Include(x => x.User)
            .Where(x => !x.IsDeleted && x.Status == DownloadRequestStatus.Pending)
            .OrderByDescending(x => x.CreatedAt);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<DownloadRequest>> GetByUserIdAsync(
        Guid userId, CancellationToken ct = default)
    {
        return await DbSet
            .Include(x => x.User)
            .Where(x => !x.IsDeleted && x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }
}
