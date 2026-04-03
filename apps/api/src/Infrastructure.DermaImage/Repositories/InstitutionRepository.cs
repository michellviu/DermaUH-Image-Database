using Domain.DermaImage.Entities;
using Domain.DermaImage.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.DermaImage.Repositories;

public class InstitutionRepository : Repository<Institution>, IInstitutionRepository
{
    public InstitutionRepository(DermaImageDbContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory) { }

    public async Task<Institution?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Fetching institution by name: {InstitutionName}", name);
        return await DbSet.FirstOrDefaultAsync(i => i.Name == name, cancellationToken);
    }
}
