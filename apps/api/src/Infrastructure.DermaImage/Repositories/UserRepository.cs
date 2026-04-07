using Domain.DermaImage.Entities;
using Domain.DermaImage.Interfaces.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.DermaImage.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<User> _userManager;
    private readonly DermaImageDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(UserManager<User> userManager, DermaImageDbContext context, ILogger<UserRepository> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching user by ID: {UserId}", id);
        return await _context.Users
            .Include(u => u.Institution)
            .Include(u => u.ContributedImages)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching user by email");
        return await _context.Users
            .Include(u => u.Institution)
            .Include(u => u.ContributedImages)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all users");
        return await _context.Users
            .Include(u => u.Institution)
            .Include(u => u.ContributedImages)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching paged users");
        var query = _context.Users.AsQueryable();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Include(u => u.Institution)
            .Include(u => u.ContributedImages)
            .OrderBy(u => u.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<User> CreateAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating user: {@User}", user);
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create user: {Errors}", errors);
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating user: {@User}", user);
        user.UpdatedAt = DateTime.UtcNow;
        
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to update user: {Errors}", errors);
            throw new InvalidOperationException($"Failed to update user: {errors}");
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);
        _logger.LogInformation("Deleting user: {@User}", user);
        if (user is not null)
        {
            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task AddToRoleAsync(User user, string role)
    {
        _logger.LogInformation("Adding user to role: {@User}, {Role}", user, role);
        await _userManager.AddToRoleAsync(user, role);
    }

    public async Task RemoveFromRoleAsync(User user, string role)
    {
        _logger.LogInformation("Removing user from role: {@User}, {Role}", user, role);
        await _userManager.RemoveFromRoleAsync(user, role);
    }

    public async Task<IList<string>> GetRolesAsync(User user)
    {
        _logger.LogInformation("Fetching roles for user: {@User}", user);
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<User> CreateExternalAsync(User user, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating external user: {@User}", user);
        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create external user: {Errors}", errors);
            throw new InvalidOperationException($"Failed to create external user: {errors}");
        }
        return user;
    }

    public Task<bool> CheckPasswordAsync(User user, string password)
    {
        _logger.LogInformation("Checking password for user: {@User}", user);
        return _userManager.CheckPasswordAsync(user, password);
    }

    public Task<string> GenerateEmailConfirmationTokenAsync(User user)
    {
        _logger.LogInformation("Generating email confirmation token for user: {@User}", user);
        return _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
    {
        _logger.LogInformation("Confirming email for user: {@User}", user);
        return await _userManager.ConfirmEmailAsync(user, token);
    }

    public Task<string> GeneratePasswordResetTokenAsync(User user)
    {
        _logger.LogInformation("Generating password reset token for user: {@User}", user);
        return _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword)
    {
        _logger.LogInformation("Resetting password for user: {@User}", user);
        return await _userManager.ResetPasswordAsync(user, token, newPassword);
    }

    public Task<bool> HasPasswordAsync(User user)
    {
        _logger.LogInformation("Checking if user has password: {@User}", user);
        return _userManager.HasPasswordAsync(user);
    }

    public async Task<IdentityResult> AddPasswordAsync(User user, string newPassword)
    {
        _logger.LogInformation("Adding password for user: {@User}", user);
        return await _userManager.AddPasswordAsync(user, newPassword);
    }

    public async Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword)
    {
        _logger.LogInformation("Changing password for user: {@User}", user);
        return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
    }

    public Task<User?> FindByLoginAsync(string loginProvider, string providerKey)
    {
        _logger.LogInformation("Finding user by external login. Provider: {LoginProvider}, Key: {ProviderKey}", loginProvider, providerKey);
        return _userManager.FindByLoginAsync(loginProvider, providerKey);
    }

    public Task<IdentityResult> AddLoginAsync(User user, UserLoginInfo loginInfo)
    {
        _logger.LogInformation("Adding external login for user: {@User}. Provider: {LoginProvider}", user, loginInfo.LoginProvider);
        return _userManager.AddLoginAsync(user, loginInfo);
    }


}
