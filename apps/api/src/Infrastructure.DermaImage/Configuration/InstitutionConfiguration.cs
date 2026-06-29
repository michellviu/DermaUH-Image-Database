using Domain.DermaImage.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DermaImage.Configuration;

public class InstitutionConfiguration : IEntityTypeConfiguration<Institution>
{
    public void Configure(EntityTypeBuilder<Institution> builder)
    {
        builder.ToTable("Institutions");

        // Standard Guid primary key (inherited from BaseEntity)
        builder.HasKey(e => e.Id);

        // Name must be unique — used as the natural lookup key
        builder.HasIndex(e => e.Name).IsUnique();
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);

        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.Country).HasMaxLength(100);

        // Soft delete filter (inherited from BaseEntity)
        builder.HasQueryFilter(e => !e.IsDeleted);

        // One Institution → Many DermaImgs (via Guid FK)
        builder.HasMany(e => e.Images)
               .WithOne(i => i.Institution)
               .HasForeignKey(i => i.InstitutionId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
