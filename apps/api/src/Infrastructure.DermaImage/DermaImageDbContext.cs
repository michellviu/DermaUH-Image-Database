using Domain.DermaImage.Entities;
using Domain.DermaImage.Entities.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DermaImage;

public class DermaImageDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public DermaImageDbContext(DbContextOptions<DermaImageDbContext> options) : base(options) { }

    public DbSet<DermaImg> Images => Set<DermaImg>();
    public DbSet<Institution> Institutions => Set<Institution>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Rename Identity tables to cleaner names
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<IdentityRole<Guid>>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

        // Seed roles with deterministic GUIDs (required by EF Core HasData)
        modelBuilder.Entity<IdentityRole<Guid>>().HasData(
            new IdentityRole<Guid> { Id = Guid.Parse("a1b2c3d4-0001-0000-0000-000000000001"), Name = nameof(UserRole.Viewer), NormalizedName = nameof(UserRole.Viewer).ToUpperInvariant() },
            new IdentityRole<Guid> { Id = Guid.Parse("a1b2c3d4-0002-0000-0000-000000000002"), Name = nameof(UserRole.Contributor), NormalizedName = nameof(UserRole.Contributor).ToUpperInvariant() },
            new IdentityRole<Guid> { Id = Guid.Parse("a1b2c3d4-0003-0000-0000-000000000003"), Name = nameof(UserRole.Reviewer), NormalizedName = nameof(UserRole.Reviewer).ToUpperInvariant() },
            new IdentityRole<Guid> { Id = Guid.Parse("a1b2c3d4-0004-0000-0000-000000000004"), Name = nameof(UserRole.Admin), NormalizedName = nameof(UserRole.Admin).ToUpperInvariant() }
        );

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DermaImageDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        // Also handle User audit fields (not BaseEntity)
        foreach (var entry in ChangeTracker.Entries<User>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
