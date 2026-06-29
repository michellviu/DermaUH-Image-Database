using Application.DermaImage.DTOs;
using Domain.DermaImage.Entities;

namespace Application.DermaImage.Managers;

public interface IInstitutionManager
{
    Task<IReadOnlyList<InstitutionResponseDto>> GetInstitutionsAsync(bool includePrivate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the existing institution with the given name, or creates a new one if not found.
    /// </summary>
    Task<Institution> GetOrCreateAsync(string name, string? description, string? country, CancellationToken cancellationToken = default);
}
