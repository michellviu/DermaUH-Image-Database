using Domain.DermaImage.Entities;
using Domain.DermaImage.Interfaces.Repository;
using Domain.DermaImage.Interfaces.Services;

namespace Application.DermaImage.Services;

public class InstitutionService : IInstitutionService
{
    private readonly IInstitutionRepository _repository;

    public InstitutionService(IInstitutionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Institution?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<(IEnumerable<Institution> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _repository.GetPagedAsync(page, pageSize, cancellationToken: cancellationToken);
    }

    public async Task<Institution> CreateAsync(Institution institution, CancellationToken cancellationToken = default)
    {
        return await _repository.AddAsync(institution, cancellationToken);
    }

    public async Task UpdateAsync(Institution institution, CancellationToken cancellationToken = default)
    {
        await _repository.UpdateAsync(institution, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteAsync(id, cancellationToken);
    }
}
