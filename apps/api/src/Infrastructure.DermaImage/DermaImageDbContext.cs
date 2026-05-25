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

        // Seed roles with fully deterministic values (ConcurrencyStamp must be static,
        // otherwise IdentityRole's constructor calls Guid.NewGuid() on every build,
        // triggering PendingModelChangesWarning).
        modelBuilder.Entity<IdentityRole<Guid>>().HasData(
            new IdentityRole<Guid> { Id = Guid.Parse("a1b2c3d4-0001-0000-0000-000000000001"), Name = nameof(UserRole.Viewer),      NormalizedName = nameof(UserRole.Viewer).ToUpperInvariant(),      ConcurrencyStamp = "d3c8b1ea-f6a1-473b-ad63-b536b4cffcfc" },
            new IdentityRole<Guid> { Id = Guid.Parse("a1b2c3d4-0002-0000-0000-000000000002"), Name = nameof(UserRole.Contributor), NormalizedName = nameof(UserRole.Contributor).ToUpperInvariant(), ConcurrencyStamp = "caa0f0fa-b5e0-4d66-9f6f-f15e69c9f15b" },
            new IdentityRole<Guid> { Id = Guid.Parse("a1b2c3d4-0004-0000-0000-000000000004"), Name = nameof(UserRole.Admin),       NormalizedName = nameof(UserRole.Admin).ToUpperInvariant(),       ConcurrencyStamp = "27804fd8-0e9d-4ea2-af30-e638e994c174" }
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
