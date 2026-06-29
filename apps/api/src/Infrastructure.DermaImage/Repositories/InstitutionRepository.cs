using Domain.DermaImage.Entities;
using Domain.DermaImage.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.DermaImage.Repositories;

public class InstitutionRepository : IInstitutionRepository
{
    private readonly DermaImageDbContext _context;
    private readonly ILogger<InstitutionRepository> _logger;

    public InstitutionRepository(DermaImageDbContext context, ILogger<InstitutionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task<Institution?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching institution by name: {Name}", name);
        return _context.Institutions
            .FirstOrDefaultAsync(i => i.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyList<Institution>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all institutions");
        return await _context.Institutions
            .OrderBy(i => i.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Institution> AddAsync(Institution institution, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding institution: {Name}", institution.Name);
        _context.Institutions.Add(institution);
        await _context.SaveChangesAsync(cancellationToken);
        return institution;
    }

    public async Task<IReadOnlyList<(Guid InstitutionId, int ImageCount)>> GetImageCountsAsync(
        bool includePrivate, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching institution image counts. IncludePrivate: {IncludePrivate}", includePrivate);

        var query = _context.Images.AsNoTracking()
            .Where(i => !i.IsDeleted && i.InstitutionId.HasValue);

        if (!includePrivate)
        {
            query = query.Where(i => i.IsPublic);
        }

        var grouped = await query
            .GroupBy(i => i.InstitutionId!.Value)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return grouped.Select(x => (x.Id, x.Count)).ToList();
    }
}
