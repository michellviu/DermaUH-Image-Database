using Domain.DermaImage.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DermaImage.Configuration;

public class InstitutionJoinRequestConfiguration : IEntityTypeConfiguration<InstitutionJoinRequest>
{
    public void Configure(EntityTypeBuilder<InstitutionJoinRequest> builder)
    {
        builder.ToTable("InstitutionJoinRequests");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ReviewComment)
            .HasMaxLength(500);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasIndex(e => new { e.ApplicantUserId, e.InstitutionId, e.Status });

        builder.HasOne(e => e.Institution)
            .WithMany(i => i.JoinRequests)
            .HasForeignKey(e => e.InstitutionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ApplicantUser)
            .WithMany(u => u.InstitutionJoinRequests)
            .HasForeignKey(e => e.ApplicantUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ReviewedByUser)
            .WithMany(u => u.ReviewedInstitutionJoinRequests)
            .HasForeignKey(e => e.ReviewedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}