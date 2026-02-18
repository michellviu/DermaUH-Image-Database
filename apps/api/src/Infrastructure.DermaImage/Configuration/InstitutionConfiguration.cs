using Domain.DermaImage.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DermaImage.Configuration;

public class InstitutionConfiguration : IEntityTypeConfiguration<Institution>
{
    public void Configure(EntityTypeBuilder<Institution> builder)
    {
        builder.ToTable("Institutions");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.Name).IsUnique();
        builder.Property(e => e.Name).IsRequired().HasMaxLength(300);

        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.Country).HasMaxLength(100);
        builder.Property(e => e.City).HasMaxLength(100);
        builder.Property(e => e.Address).HasMaxLength(500);
        builder.Property(e => e.Website).HasMaxLength(500);
        builder.Property(e => e.ContactEmail).HasMaxLength(256);
        builder.Property(e => e.LogoUrl).HasMaxLength(1000);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
