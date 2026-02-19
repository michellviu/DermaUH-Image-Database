using Domain.DermaImage.Entities;
using Domain.DermaImage.Interfaces.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DermaImage.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<User> _userManager;
    private readonly DermaImageDbContext _context;

    public UserRepository(UserManager<User> userManager, DermaImageDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Institution)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Institution)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Institution)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsQueryable();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Include(u => u.Institution)
            .OrderBy(u => u.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<User> CreateAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        user.UpdatedAt = DateTime.UtcNow;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to update user: {errors}");
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);
        if (user is not null)
        {
            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task AddToRoleAsync(User user, string role)
    {
        await _userManager.AddToRoleAsync(user, role);
    }

    public async Task<IList<string>> GetRolesAsync(User user)
    {
        return await _userManager.GetRolesAsync(user);
    }
}
