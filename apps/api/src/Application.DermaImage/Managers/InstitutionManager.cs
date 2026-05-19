using Application.DermaImage.DTOs;
using Domain.DermaImage.Interfaces.Repository;

namespace Application.DermaImage.Managers;

public class InstitutionManager : IInstitutionManager
{
    private readonly IDermaImgRepository _repository;

    public InstitutionManager(IDermaImgRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<InstitutionResponseDto>> GetInstitutionsAsync(bool includePrivate, CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetDerivedInstitutionsAsync(includePrivate, cancellationToken);
        return result.Select(x => new InstitutionResponseDto
        {
            Name = x.InstitutionName,
            Description = x.InstitutionDescription,
            Country = x.InstitutionCountry,
            ImageCount = x.Count
        }).ToList();
    }
}
