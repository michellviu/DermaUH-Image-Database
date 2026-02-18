using Domain.DermaImage.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DermaImage.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Table name is set in DbContext (Identity table renaming)

        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);

        builder.Ignore(e => e.FullName);

        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasOne(e => e.Institution)
            .WithMany(i => i.Users)
            .HasForeignKey(e => e.InstitutionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
