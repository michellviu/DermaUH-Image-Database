using Domain.DermaImage.Entities;
using Domain.DermaImage.Entities.Enums;
using Microsoft.AspNetCore.Identity;

namespace WebApi.DermaImage.Startup;

/// <summary>
/// Creates the first administrator of a fresh deployment from the
/// <c>BootstrapAdmin</c> configuration section. Without it, nobody could
/// approve new registrations, since every self-registered account starts
/// inactive and there is no seeded Admin user.
/// </summary>
public static class AdminBootstrapper
{
    public static async Task EnsureAdminAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        var section = configuration.GetSection("BootstrapAdmin");
        var email = section["Email"];
        var password = section["Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var userManager = services.GetRequiredService<UserManager<User>>();
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new User
            {
                FirstName = section["FirstName"] ?? "Administrador",
                LastName = section["LastName"] ?? "DermaUH",
                Email = email,
                UserName = email,
                EmailConfirmed = true,
                IsActive = true,
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                logger.LogError(
                    "Bootstrap admin {Email} could not be created: {Errors}",
                    email,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }

            logger.LogInformation("Bootstrap admin {Email} created", email);
        }

        if (!await userManager.IsInRoleAsync(user, nameof(UserRole.Admin)))
        {
            await userManager.AddToRoleAsync(user, nameof(UserRole.Admin));
            logger.LogInformation("Admin role granted to bootstrap user {Email}", email);
        }
    }
}
