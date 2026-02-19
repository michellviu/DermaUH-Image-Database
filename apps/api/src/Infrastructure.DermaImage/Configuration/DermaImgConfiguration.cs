using Domain.DermaImage.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DermaImage.Configuration;

public class DermaImgConfiguration : IEntityTypeConfiguration<DermaImg>
{
    public void Configure(EntityTypeBuilder<DermaImg> builder)
    {
        builder.ToTable("Images");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.PublicId).IsUnique();
        builder.Property(e => e.PublicId).IsRequired().HasMaxLength(30);

        builder.Property(e => e.FileName).IsRequired().HasMaxLength(500);
        builder.Property(e => e.FilePath).IsRequired().HasMaxLength(1000);
        builder.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Attribution).HasMaxLength(500);
        builder.Property(e => e.Diagnosis).HasMaxLength(500);
        builder.Property(e => e.DiagnosisLevel2).HasMaxLength(500);
        builder.Property(e => e.DiagnosisLevel3).HasMaxLength(500);
        builder.Property(e => e.DiagnosisLevel4).HasMaxLength(500);
        builder.Property(e => e.DiagnosisLevel5).HasMaxLength(500);
        builder.Property(e => e.ClinicalNotes).HasMaxLength(2000);

        // Enum conversions stored as strings
        builder.Property(e => e.CopyrightLicense).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.ImageType).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.ImageManipulation).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.DermoscopicType).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.Sex).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.AnatomSiteGeneral).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.AnatomSiteSpecial).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.DiagnosisCategory).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.DiagnosisConfirmType).HasConversion<string>().HasMaxLength(100);
        builder.Property(e => e.MelMitoticIndex).HasConversion<string>().HasMaxLength(50);

        // Soft delete filter
        builder.HasQueryFilter(e => !e.IsDeleted);

        // Relationships
        builder.HasOne(e => e.Contributor)
            .WithMany(u => u.ContributedImages)
            .HasForeignKey(e => e.ContributorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Institution)
            .WithMany(i => i.Images)
            .HasForeignKey(e => e.InstitutionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
