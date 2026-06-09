using Application.DermaImage.DTOs;
using Domain.DermaImage.Entities;
using Domain.DermaImage.Entities.Enums;
using Domain.DermaImage.Interfaces.Repository;
using Domain.DermaImage.Interfaces.Services;

namespace Application.DermaImage.Managers;

public class DownloadManager : IDownloadManager
{
    private readonly IDownloadRequestRepository _requestRepo;
    private readonly IUserRepository _userRepo;
    private readonly IEmailService _emailService;

    public DownloadManager(
        IDownloadRequestRepository requestRepo,
        IUserRepository userRepo,
        IEmailService emailService)
    {
        _requestRepo = requestRepo;
        _userRepo = userRepo;
        _emailService = emailService;
    }

    public async Task<bool> HasActiveAuthorizationAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        return user is not null && user.IsDownloadAuthorized;
    }

    public async Task<DownloadRequestResponseDto> CreateRequestAsync(
        Guid userId, CreateDownloadRequestDto dto, CancellationToken ct = default)
    {
        var request = new DownloadRequest
        {
            UserId = userId,
            Reason = dto.Reason,
            Institution = dto.Institution,
            Status = DownloadRequestStatus.Pending
        };

        var created = await _requestRepo.AddAsync(request, ct);

        var admins = await _userRepo.GetActiveUsersByRoleAsync(UserRole.Admin, ct);
        var adminEmails = admins
            .Select(a => a.Email)
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Cast<string>()
            .ToList();

        if (adminEmails.Count > 0)
        {
            var user = await _userRepo.GetByIdAsync(userId, ct);
            await _emailService.SendAdminNotificationNewDownloadRequestAsync(
                adminEmails, user?.FullName ?? "Unknown", ct);
        }

        return MapToDto(created);
    }

    public async Task<(IEnumerable<DownloadRequestResponseDto> Items, int TotalCount)> GetPendingRequestsAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var (items, totalCount) = await _requestRepo.GetPendingPagedAsync(page, pageSize, ct);
        var dtos = new List<DownloadRequestResponseDto>();
        foreach (var item in items)
        {
            dtos.Add(MapToDto(item));
        }
        return (dtos, totalCount);
    }

    public async Task<IReadOnlyList<DownloadRequestResponseDto>> GetRequestsByUserAsync(
        Guid userId, CancellationToken ct = default)
    {
        var items = await _requestRepo.GetByUserIdAsync(userId, ct);
        return items.Select(MapToDto).ToList().AsReadOnly();
    }

    public async Task ReviewRequestAsync(Guid requestId, Guid adminUserId,
        ReviewDownloadRequestDto dto, CancellationToken ct = default)
    {
        var request = await _requestRepo.GetByIdAsync(requestId, ct)
            ?? throw new KeyNotFoundException($"Download request {requestId} not found.");

        var status = Enum.Parse<DownloadRequestStatus>(dto.Status);
        request.Status = status;
        request.ReviewedById = adminUserId;
        request.ReviewedAt = DateTime.UtcNow;
        await _requestRepo.UpdateAsync(request, ct);

        var user = await _userRepo.GetByIdAsync(request.UserId, ct);
        if (user is null) return;

        if (status == DownloadRequestStatus.Approved)
        {
            user.IsDownloadAuthorized = true;
            await _userRepo.UpdateAsync(user, ct);

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                await _emailService.SendDownloadRequestApprovedAsync(
                    user.Email, user.FullName, "/images", ct);
            }
        }
        else if (status == DownloadRequestStatus.Denied)
        {
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                await _emailService.SendDownloadRequestDeniedAsync(
                    user.Email, user.FullName, ct);
            }
        }
    }

    private static DownloadRequestResponseDto MapToDto(DownloadRequest request)
    {
        return new DownloadRequestResponseDto
        {
            Id = request.Id,
            UserId = request.UserId,
            UserFullName = request.User?.FullName ?? string.Empty,
            UserEmail = request.User?.Email ?? string.Empty,
            Reason = request.Reason,
            Institution = request.Institution,
            Status = request.Status.ToString(),
            CreatedAt = request.CreatedAt,
            ReviewedById = request.ReviewedById,
            ReviewedAt = request.ReviewedAt
        };
    }
}
