using Domain.DermaImage.Entities;

namespace Domain.DermaImage.Interfaces.Services;

/// <summary>
/// Generates and validates JWT tokens.
/// </summary>
public interface IJwtService
{
    string GenerateToken(User user, IList<string> roles);
}
