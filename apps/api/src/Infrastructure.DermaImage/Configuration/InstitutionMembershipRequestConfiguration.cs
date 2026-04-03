using Domain.DermaImage.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DermaImage.Configuration;

public class InstitutionMembershipRequestConfiguration : IEntityTypeConfiguration<InstitutionMembershipRequest>
{
    public void Configure(EntityTypeBuilder<InstitutionMembershipRequest> builder)
    {
        builder.ToTable("InstitutionMembershipRequests");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ReviewMessage).HasMaxLength(1000);
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasOne(e => e.Institution)
            .WithMany(i => i.MembershipRequests)
            .HasForeignKey(e => e.InstitutionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ApplicantUser)
            .WithMany(u => u.MembershipRequestsAsApplicant)
            .HasForeignKey(e => e.ApplicantUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ReviewedByUser)
            .WithMany(u => u.MembershipRequestsReviewed)
            .HasForeignKey(e => e.ReviewedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => new { e.ApplicantUserId, e.InstitutionId })
            .HasFilter("\"Status\" = 0")
            .IsUnique();
    }
}
