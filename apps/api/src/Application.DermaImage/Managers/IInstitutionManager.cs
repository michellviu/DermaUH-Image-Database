using Application.DermaImage.DTOs;
using Domain.DermaImage.Entities;

namespace Application.DermaImage.Managers;

public interface IInstitutionManager
{
    Task<(IEnumerable<Institution> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Institution?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Institution> CreateAsync(CreateInstitutionDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, CreateInstitutionDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
