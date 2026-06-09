using Domain.DermaImage.Entities.Enums;

namespace Domain.DermaImage.Entities;

public class DownloadRequest : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Reason { get; set; } = string.Empty;
    public string Institution { get; set; } = string.Empty;
    public DownloadRequestStatus Status { get; set; } = DownloadRequestStatus.Pending;
    public Guid? ReviewedById { get; set; }
    public User? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
