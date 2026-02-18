using Domain.DermaImage.Entities;

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
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddToRoleAsync(User user, string role);
    Task<IList<string>> GetRolesAsync(User user);
}
