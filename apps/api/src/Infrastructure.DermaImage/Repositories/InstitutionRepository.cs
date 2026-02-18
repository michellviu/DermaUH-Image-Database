using Domain.DermaImage.Entities;
using Domain.DermaImage.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DermaImage.Repositories;

public class InstitutionRepository : Repository<Institution>, IInstitutionRepository
{
    public InstitutionRepository(DermaImageDbContext context) : base(context) { }

    public async Task<Institution?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(i => i.Name == name, cancellationToken);
    }
}
