using Domain.DermaImage.Entities;
using Domain.DermaImage.Entities.Enums;
using Domain.DermaImage.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DermaImage.Repositories;

public class InstitutionMembershipRepository : IInstitutionMembershipRepository
{
    private readonly DermaImageDbContext _context;

    public InstitutionMembershipRepository(DermaImageDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsInstitutionResponsibleAsync(Guid institutionId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.InstitutionResponsibles
            .AnyAsync(x => x.InstitutionId == institutionId && x.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<InstitutionResponsible>> GetInstitutionResponsiblesAsync(Guid institutionId, CancellationToken cancellationToken = default)
    {
        return await _context.InstitutionResponsibles
            .Include(x => x.User)
            .Where(x => x.InstitutionId == institutionId)
            .OrderBy(x => x.User!.LastName)
            .ThenBy(x => x.User!.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InstitutionResponsible>> GetResponsibilitiesByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.InstitutionResponsibles
            .Include(x => x.Institution)
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddInstitutionResponsibleAsync(InstitutionResponsible assignment, CancellationToken cancellationToken = default)
    {
        var existingAssignment = await _context.InstitutionResponsibles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                x => x.InstitutionId == assignment.InstitutionId && x.UserId == assignment.UserId,
                cancellationToken);

        if (existingAssignment is not null)
        {
            if (!existingAssignment.IsDeleted)
            {
                throw new InvalidOperationException("El usuario ya es responsable de esta institución.");
            }

            existingAssignment.IsDeleted = false;
            existingAssignment.AssignedByUserId = assignment.AssignedByUserId;
            existingAssignment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        await _context.InstitutionResponsibles.AddAsync(assignment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveInstitutionResponsibleAsync(Guid institutionId, Guid userId, CancellationToken cancellationToken = default)
    {
        var assignment = await _context.InstitutionResponsibles
            .FirstOrDefaultAsync(x => x.InstitutionId == institutionId && x.UserId == userId, cancellationToken);

        if (assignment is null)
        {
            return;
        }

        assignment.IsDeleted = true;
        assignment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<InstitutionJoinRequest?> GetJoinRequestByIdAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        return await _context.InstitutionJoinRequests
            .Include(x => x.Institution)
            .Include(x => x.ApplicantUser)
            .Include(x => x.ReviewedByUser)
            .FirstOrDefaultAsync(x => x.Id == requestId, cancellationToken);
    }

    public async Task<InstitutionJoinRequest?> GetPendingJoinRequestAsync(Guid applicantUserId, Guid institutionId, CancellationToken cancellationToken = default)
    {
        return await _context.InstitutionJoinRequests
            .FirstOrDefaultAsync(x =>
                x.ApplicantUserId == applicantUserId
                && x.InstitutionId == institutionId
                && x.Status == InstitutionJoinRequestStatus.Pending,
                cancellationToken);
    }

    public async Task<(IReadOnlyList<InstitutionJoinRequest> Items, int TotalCount)> GetJoinRequestsByUserAsync(Guid applicantUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.InstitutionJoinRequests
            .Include(x => x.Institution)
            .Include(x => x.ReviewedByUser)
            .Where(x => x.ApplicantUserId == applicantUserId);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<InstitutionJoinRequest> Items, int TotalCount)> GetInboxJoinRequestsAsync(IEnumerable<Guid> institutionIds, int page, int pageSize, bool includeReviewed = false, CancellationToken cancellationToken = default)
    {
        var ids = institutionIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return ([], 0);
        }

        var query = _context.InstitutionJoinRequests
            .Include(x => x.Institution)
            .Include(x => x.ApplicantUser)
            .Include(x => x.ReviewedByUser)
            .Where(x => ids.Contains(x.InstitutionId));

        if (!includeReviewed)
        {
            query = query.Where(x => x.Status == InstitutionJoinRequestStatus.Pending);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddJoinRequestAsync(InstitutionJoinRequest joinRequest, CancellationToken cancellationToken = default)
    {
        await _context.InstitutionJoinRequests.AddAsync(joinRequest, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateJoinRequestAsync(InstitutionJoinRequest joinRequest, CancellationToken cancellationToken = default)
    {
        _context.InstitutionJoinRequests.Update(joinRequest);
        await _context.SaveChangesAsync(cancellationToken);
    }
}