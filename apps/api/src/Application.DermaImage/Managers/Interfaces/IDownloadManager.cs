using Application.DermaImage.DTOs;

namespace Application.DermaImage.Managers;

public interface IDownloadManager
{
    Task<bool> HasActiveAuthorizationAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<DownloadRequestResponseDto> CreateRequestAsync(Guid userId, CreateDownloadRequestDto dto, CancellationToken cancellationToken = default);
    Task<(IEnumerable<DownloadRequestResponseDto> Items, int TotalCount)> GetPendingRequestsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DownloadRequestResponseDto>> GetRequestsByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task ReviewRequestAsync(Guid requestId, Guid adminUserId, ReviewDownloadRequestDto dto, CancellationToken cancellationToken = default);
}
