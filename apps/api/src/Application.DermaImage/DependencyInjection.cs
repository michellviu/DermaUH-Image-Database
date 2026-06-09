using Domain.DermaImage.Interfaces.Services;
using Application.DermaImage.Managers;
using Application.DermaImage.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application.DermaImage;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Services
        services.AddScoped<IDermaImgService, DermaImgService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDermaImgManager, DermaImgManager>();
        services.AddScoped<IUserManager, Managers.UserManager>();
        services.AddScoped<IInstitutionManager, InstitutionManager>();
        services.AddScoped<IAuthManager, AuthManager>();
        services.AddScoped<IStatisticsManager, StatisticsManager>();
        services.AddScoped<IDownloadManager, DownloadManager>();

        return services;
    }
}
