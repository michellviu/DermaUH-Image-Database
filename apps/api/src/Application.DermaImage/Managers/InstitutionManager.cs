using Application.DermaImage.DTOs;
using Domain.DermaImage.Entities;
using Domain.DermaImage.Interfaces.Repository;

namespace Application.DermaImage.Managers;

public class InstitutionManager : IInstitutionManager
{
    private readonly IInstitutionRepository _repository;

    public InstitutionManager(IInstitutionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<InstitutionResponseDto>> GetInstitutionsAsync(
        bool includePrivate, CancellationToken cancellationToken = default)
    {
        var institutions = await _repository.GetAllAsync(cancellationToken);
        var counts = await _repository.GetImageCountsAsync(includePrivate, cancellationToken);

        var countMap = counts.ToDictionary(x => x.InstitutionId, x => x.ImageCount);

        return institutions
            .Select(i => new InstitutionResponseDto
            {
                Name        = i.Name,
                Description = i.Description,
                Country     = i.Country,
                ImageCount  = countMap.TryGetValue(i.Id, out var c) ? c : 0
            })
            .OrderByDescending(i => i.ImageCount)
            .ToList();
    }

    /// <summary>
    /// Returns the existing institution that matches <paramref name="name"/> (case-sensitive),
    /// or creates a new one with the supplied description and country if none is found.
    /// </summary>
    public async Task<Institution> GetOrCreateAsync(
        string name,
        string? description,
        string? country,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByNameAsync(name, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var institution = new Institution
        {
            Name        = name,
            Description = description,
            Country     = country
        };

        return await _repository.AddAsync(institution, cancellationToken);
    }
}
