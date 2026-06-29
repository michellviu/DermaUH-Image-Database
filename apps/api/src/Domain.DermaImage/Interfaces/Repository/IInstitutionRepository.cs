using Domain.DermaImage.Entities;

namespace Domain.DermaImage.Interfaces.Repository;

public interface IInstitutionRepository
{
    /// <summary>Finds an institution by its unique name. Returns null if not found.</summary>
    Task<Institution?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Returns all institutions (excluding soft-deleted).</summary>
    Task<IReadOnlyList<Institution>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Persists a new institution and returns the saved entity (with generated Id).</summary>
    Task<Institution> AddAsync(Institution institution, CancellationToken cancellationToken = default);

    /// <summary>Returns the image count per institution, respecting visibility.</summary>
    Task<IReadOnlyList<(Guid InstitutionId, int ImageCount)>> GetImageCountsAsync(bool includePrivate, CancellationToken cancellationToken = default);
}
