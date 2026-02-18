using Domain.DermaImage.Entities;

namespace Domain.DermaImage.Interfaces.Services;

public interface IInstitutionService
{
    Task<Institution?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Institution> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Institution> CreateAsync(Institution institution, CancellationToken cancellationToken = default);
    Task UpdateAsync(Institution institution, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
