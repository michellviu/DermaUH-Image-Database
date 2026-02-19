using Application.DermaImage.DTOs;
using Domain.DermaImage.Entities;
using Domain.DermaImage.Entities.Enums;
using Domain.DermaImage.Interfaces.Services;

namespace Application.DermaImage.Managers;

public class UserManager : IUserManager
{
    private readonly IUserService _service;

    public UserManager(IUserService service)
    {
        _service = service;
    }

    public async Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _service.GetPagedAsync(page, pageSize, cancellationToken);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _service.GetByIdAsync(id, cancellationToken);
    }

    public async Task<User> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            UserName = dto.Email,
            InstitutionId = dto.InstitutionId
        };

        var created = await _service.CreateAsync(user, dto.Password, cancellationToken);

        var role = UserRole.Viewer;
        await _service.AddToRoleAsync(created, role.ToString());

        return created;
    }

    public async Task<IList<string>> GetRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _service.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException($"User with id '{userId}' was not found.");

        return await _service.GetRolesAsync(user);
    }

    public async Task AssignRoleAsync(Guid userId, UserRole role, CancellationToken cancellationToken = default)
    {
        var user = await _service.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException($"User with id '{userId}' was not found.");

        await _service.AddToRoleAsync(user, role.ToString());
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _service.DeleteAsync(id, cancellationToken);
    }
}
