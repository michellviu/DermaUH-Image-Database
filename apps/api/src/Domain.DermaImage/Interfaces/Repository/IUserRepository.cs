using Domain.DermaImage.Entities;
using Microsoft.AspNetCore.Identity;

namespace Domain.DermaImage.Interfaces.Repository;

/// <summary>
/// Repository for User operations. Standalone interface (not generic)
/// because User inherits from IdentityUser, not BaseEntity.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<User> CreateAsync(User user, string password, CancellationToken cancellationToken = default);
    Task<User> CreateExternalAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetUsersInRoleAsync(string role, CancellationToken cancellationToken = default);
    Task AddToRoleAsync(User user, string role);
    Task<IList<string>> GetRolesAsync(User user);

    // Auth-specific
    Task<bool> CheckPasswordAsync(User user, string password);
    Task<string> GenerateEmailConfirmationTokenAsync(User user);
    Task<IdentityResult> ConfirmEmailAsync(User user, string token);
    Task<string> GeneratePasswordResetTokenAsync(User user);
    Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword);
    Task<bool> HasPasswordAsync(User user);
    Task<IdentityResult> AddPasswordAsync(User user, string newPassword);
    Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword);
    Task<User?> FindByLoginAsync(string loginProvider, string providerKey);
    Task<IdentityResult> AddLoginAsync(User user, UserLoginInfo loginInfo);
}
