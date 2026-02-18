using Domain.DermaImage.Entities;
using Domain.DermaImage.Interfaces.Repository;
using Domain.DermaImage.Interfaces.Services;

namespace Application.DermaImage.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByEmailAsync(email, cancellationToken);
    }

    public async Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _repository.GetPagedAsync(page, pageSize, cancellationToken);
    }

    public async Task<User> CreateAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        return await _repository.CreateAsync(user, password, cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        await _repository.UpdateAsync(user, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task AddToRoleAsync(User user, string role)
    {
        await _repository.AddToRoleAsync(user, role);
    }

    public async Task<IList<string>> GetRolesAsync(User user)
    {
        return await _repository.GetRolesAsync(user);
    }
}
