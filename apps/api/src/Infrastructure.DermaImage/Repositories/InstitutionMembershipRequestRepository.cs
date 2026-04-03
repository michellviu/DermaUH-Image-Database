using Domain.DermaImage.Entities;
using Domain.DermaImage.Entities.Enums;
using Domain.DermaImage.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.DermaImage.Repositories;

public class InstitutionMembershipRequestRepository : Repository<InstitutionMembershipRequest>, IInstitutionMembershipRequestRepository
{
    public InstitutionMembershipRequestRepository(DermaImageDbContext context, ILoggerFactory loggerFactory) : base(context, loggerFactory)
    {
    }

    public async Task<InstitutionMembershipRequest?> GetPendingAsync(Guid applicantUserId, Guid institutionId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(x =>
                x.ApplicantUserId == applicantUserId
                && x.InstitutionId == institutionId
                && x.Status == InstitutionMembershipRequestStatus.Pending,
                cancellationToken);
    }

    public async Task<IEnumerable<InstitutionMembershipRequest>> GetByApplicantAsync(Guid applicantUserId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(x => x.Institution)
            .Include(x => x.ReviewedByUser)
            .Where(x => x.ApplicantUserId == applicantUserId)
            .OrderByDescending(x => x.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<InstitutionMembershipRequest>> GetPendingForInstitutionAsync(Guid institutionId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(x => x.ApplicantUser)
            .Where(x => x.InstitutionId == institutionId && x.Status == InstitutionMembershipRequestStatus.Pending)
            .OrderByDescending(x => x.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<InstitutionMembershipRequest?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(x => x.Institution)
            .Include(x => x.ApplicantUser)
            .Include(x => x.ReviewedByUser)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> HasAnyResponsibleReviewedAsync(Guid applicantUserId, Guid institutionId, InstitutionMembershipRequestStatus status, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(x =>
            x.ApplicantUserId == applicantUserId
            && x.InstitutionId == institutionId
            && x.Status == status, cancellationToken);
    }
}
