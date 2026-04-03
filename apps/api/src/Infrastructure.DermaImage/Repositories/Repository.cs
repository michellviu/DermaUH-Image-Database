using System.Linq.Expressions;
using Domain.DermaImage.Entities;
using Domain.DermaImage.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.DermaImage.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly DermaImageDbContext Context;
    protected readonly DbSet<T> DbSet;
    protected readonly ILogger Logger;

    public Repository(DermaImageDbContext context, ILoggerFactory loggerFactory)
    {
        Context = context;
        DbSet = context.Set<T>();
        Logger = loggerFactory.CreateLogger($"{GetType().Name}:{typeof(T).Name}");
    }

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Fetching {EntityName} by id: {Id}", typeof(T).Name, id);
        return DbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Fetching all {EntityName}", typeof(T).Name);
        return await DbSet.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Finding {EntityName} with custom predicate", typeof(T).Name);
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Adding {EntityName}: {@Entity}", typeof(T).Name, entity);
        var entry = await DbSet.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        Logger.LogInformation("Added {EntityName} with id: {Id}", typeof(T).Name, entry.Entity.Id);
        return entry.Entity;
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Updating {EntityName}: {@Entity}", typeof(T).Name, entity);
        DbSet.Update(entity);
        await Context.SaveChangesAsync(cancellationToken);
        Logger.LogInformation("Updated {EntityName} with id: {Id}", typeof(T).Name, entity.Id);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Deleting {EntityName} with id: {Id}", typeof(T).Name, id);
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = true;
            await Context.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("Soft deleted {EntityName} with id: {Id}", typeof(T).Name, id);
        }
        else
        {
            Logger.LogWarning("{EntityName} with id {Id} was not found for deletion", typeof(T).Name, id);
        }
    }

    public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Counting {EntityName}. Predicate provided: {HasPredicate}", typeof(T).Name, predicate is not null);
        return predicate is null
            ? DbSet.CountAsync(cancellationToken)
            : DbSet.CountAsync(predicate, cancellationToken);
    }

    public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation(
            "Fetching paged {EntityName}. Page: {Page}, PageSize: {PageSize}, Predicate provided: {HasPredicate}",
            typeof(T).Name,
            page,
            pageSize,
            predicate is not null);

        var query = predicate is null ? DbSet : DbSet.Where(predicate);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        Logger.LogInformation(
            "Fetched paged {EntityName}. Returned: {Count}, TotalCount: {TotalCount}",
            typeof(T).Name,
            items.Count,
            totalCount);

        return (items, totalCount);
    }
}
