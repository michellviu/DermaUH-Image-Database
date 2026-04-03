using System.Text;
using Domain.DermaImage.Entities;
using Domain.DermaImage.Interfaces.Repository;
using Domain.DermaImage.Interfaces.Services;
using Infrastructure.DermaImage.Repositories;
using Infrastructure.DermaImage.Services;
using Infrastructure.DermaImage.Services.Emailing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.DermaImage;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DermaImageDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Configure ASP.NET Identity
        services.AddIdentity<User, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false; // controlled in AuthManager
            })
            .AddEntityFrameworkStores<DermaImageDbContext>()
            .AddDefaultTokenProviders();

        // Configure JWT Bearer Authentication
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]
            ?? throw new InvalidOperationException("JwtSettings:SecretKey is required.");

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = jwtSettings["Issuer"],
                    ValidAudience            = jwtSettings["Audience"],
                    IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew                = TimeSpan.FromMinutes(1),
                };
            });

        services.AddAuthorizationCore();

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IDermaImgRepository, DermaImgRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IInstitutionRepository, InstitutionRepository>();
        services.AddScoped<IInstitutionMembershipRequestRepository, InstitutionMembershipRequestRepository>();

        // Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddOptions<EmailSettings>()
            .Bind(configuration.GetSection("EmailSettings"));

        services.PostConfigure<EmailSettings>(settings =>
        {
            var legacy = configuration.GetSection("Email:Smtp");

            settings.SmtpServerAddress = FirstOrDefault(settings.SmtpServerAddress, legacy["Host"]);
            settings.SmtpUserName = FirstOrDefault(settings.SmtpUserName, legacy["Username"]);
            settings.SmtpPassword = FirstOrDefault(settings.SmtpPassword, legacy["Password"]);
            settings.EmailAddress = FirstOrDefault(settings.EmailAddress, legacy["SenderEmail"], "noreply@dermauh.cu");
            settings.EmailAddressDisplay = FirstOrDefault(settings.EmailAddressDisplay, legacy["SenderName"], "DermaUH Images");

            if (settings.SmtpServerPort <= 0)
            {
                settings.SmtpServerPort = int.TryParse(legacy["Port"], out var port) ? port : 587;
            }

            if (!settings.EnableSSL && bool.TryParse(legacy["UseSsl"], out var useSsl))
            {
                settings.EnableSSL = useSsl;
            }
        });

        services.AddScoped<IEmailService, DotNetSmtpClientEmailSender>();

        return services;
    }

    private static string FirstOrDefault(params string?[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (!string.IsNullOrWhiteSpace(candidate))
                return candidate;
        }

        return string.Empty;
    }
}
