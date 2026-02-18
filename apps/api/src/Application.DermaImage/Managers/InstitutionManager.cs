using Application.DermaImage.DTOs;
using Domain.DermaImage.Entities;
using Domain.DermaImage.Interfaces.Services;

namespace Application.DermaImage.Managers;

public class InstitutionManager : IInstitutionManager
{
    private readonly IInstitutionService _service;

    public InstitutionManager(IInstitutionService service)
    {
        _service = service;
    }

    public async Task<(IEnumerable<Institution> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _service.GetPagedAsync(page, pageSize, cancellationToken);
    }

    public async Task<Institution?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _service.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Institution> CreateAsync(CreateInstitutionDto dto, CancellationToken cancellationToken = default)
    {
        var institution = new Institution
        {
            Name = dto.Name,
            Description = dto.Description,
            Country = dto.Country,
            City = dto.City,
            Address = dto.Address,
            Website = dto.Website,
            ContactEmail = dto.ContactEmail
        };

        return await _service.CreateAsync(institution, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, CreateInstitutionDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _service.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Institution with id '{id}' was not found.");

        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.Country = dto.Country;
        existing.City = dto.City;
        existing.Address = dto.Address;
        existing.Website = dto.Website;
        existing.ContactEmail = dto.ContactEmail;

        await _service.UpdateAsync(existing, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _service.DeleteAsync(id, cancellationToken);
    }
}
