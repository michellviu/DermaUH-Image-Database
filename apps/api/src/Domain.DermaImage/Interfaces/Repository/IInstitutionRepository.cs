using Domain.DermaImage.Entities;

namespace Domain.DermaImage.Interfaces.Repository;

public interface IInstitutionRepository : IRepository<Institution>
{
    Task<Institution?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
