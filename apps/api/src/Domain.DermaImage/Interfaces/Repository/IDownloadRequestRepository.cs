using Domain.DermaImage.Entities;

namespace Domain.DermaImage.Interfaces.Repository;

public interface IDownloadRequestRepository : IRepository<DownloadRequest>
{
    Task<(IEnumerable<DownloadRequest> Items, int TotalCount)> GetPendingPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DownloadRequest>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
