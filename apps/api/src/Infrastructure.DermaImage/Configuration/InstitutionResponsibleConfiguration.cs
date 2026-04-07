using Domain.DermaImage.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DermaImage.Configuration;

public class InstitutionResponsibleConfiguration : IEntityTypeConfiguration<InstitutionResponsible>
{
    public void Configure(EntityTypeBuilder<InstitutionResponsible> builder)
    {
        builder.ToTable("InstitutionResponsibles");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => new { e.InstitutionId, e.UserId })
            .IsUnique();

        builder.HasOne(e => e.Institution)
            .WithMany(i => i.Responsibles)
            .HasForeignKey(e => e.InstitutionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.User)
            .WithMany(u => u.ResponsibleInstitutions)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}