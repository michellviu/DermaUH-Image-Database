using Domain.DermaImage.Entities;
using Domain.DermaImage.Interfaces.Repository;
using Domain.DermaImage.Interfaces.Services;

namespace Application.DermaImage.Services;

public class DermaImgService : IDermaImgService
{
    private readonly IDermaImgRepository _repository;

    public DermaImgService(IDermaImgRepository repository)
    {
        _repository = repository;
    }

    public async Task<DermaImg?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<DermaImg?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByPublicIdAsync(publicId, cancellationToken);
    }

    public async Task<(IEnumerable<DermaImg> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, DermaImgFilter? filter = null, CancellationToken cancellationToken = default)
    {
        return await _repository.GetPagedFilteredAsync(page, pageSize, filter, cancellationToken);
    }

    public async Task<DermaImg> CreateAsync(DermaImg image, CancellationToken cancellationToken = default)
    {
        image.PublicId = await _repository.GeneratePublicIdAsync(cancellationToken);
        return await _repository.AddAsync(image, cancellationToken);
    }

    public async Task UpdateAsync(DermaImg image, CancellationToken cancellationToken = default)
    {
        await _repository.UpdateAsync(image, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<DermaImg>> GetByContributorAsync(Guid contributorId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByContributorIdAsync(contributorId, cancellationToken);
    }

    public async Task<IEnumerable<DermaImg>> GetByInstitutionAsync(Guid institutionId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByInstitutionIdAsync(institutionId, cancellationToken);
    }
}
