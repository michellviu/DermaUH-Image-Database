using Application.DermaImage.Managers;
using Domain.DermaImage.Interfaces.Repository;
using Domain.DermaImage.Interfaces.Services;
using Infrastructure.DermaImage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Domain.DermaImage.Entities;

namespace Tests.Integration.DermaImage.Infrastructure;

/// <summary>
/// Factory de pruebas que reemplaza la base de datos PostgreSQL por una
/// base de datos InMemory de EF Core, evitando dependencias externas.
/// También desactiva las migraciones automáticas del startup y crea el esquema desde cero.
/// </summary>
public class DermaImageWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Eliminar todos los descriptores de opciones de DBContext y de conexión
            // Esto incluye IConfigureOptions<DbContextOptions<DermaImageDbContext>> que ejecuta UseNpgsql
            var descriptors = services.Where(
                d => d.ServiceType.Name.Contains("DbContextOptions") ||
                     d.ServiceType.Name.Contains("DermaImageDbContext") ||
                     d.ServiceType.Name.Contains("DbConnection"))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Registrar InMemory database con nombre único por instancia de factory
            var dbName = $"DermaUH_Test_{Guid.NewGuid()}";
            services.AddDbContext<DermaImageDbContext>(opts =>
                opts.UseInMemoryDatabase(dbName));

            // Reconstruir el identity store sobre el nuevo contexto InMemory
            services.AddIdentityCore<User>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 4;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<DermaImageDbContext>();
        });

        builder.ConfigureServices(services =>
        {
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DermaImageDbContext>();
            db.Database.EnsureCreated();
        });
    }

    /// <summary>Crea un <see cref="HttpClient"/> sin credenciales (usuario anónimo).</summary>
    public HttpClient CreateAnonymousClient() =>
        CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
}
