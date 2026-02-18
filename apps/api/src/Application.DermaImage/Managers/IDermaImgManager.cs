using Application.DermaImage.DTOs;
using Domain.DermaImage.Entities;

namespace Application.DermaImage.Managers;

public interface IDermaImgManager
{
    Task<(IEnumerable<DermaImg> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, DermaImgFilter? filter = null, CancellationToken cancellationToken = default);
    Task<DermaImg?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DermaImg?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken = default);
    Task<DermaImg> CreateAsync(CreateDermaImgDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, CreateDermaImgDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
