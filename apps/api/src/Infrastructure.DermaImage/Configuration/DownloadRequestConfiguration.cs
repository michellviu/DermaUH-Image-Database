using Domain.DermaImage.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DermaImage.Configuration;

public class DownloadRequestConfiguration : IEntityTypeConfiguration<DownloadRequest>
{
    public void Configure(EntityTypeBuilder<DownloadRequest> builder)
    {
        builder.ToTable("DownloadRequests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.Institution).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ReviewedBy).WithMany().HasForeignKey(x => x.ReviewedById).OnDelete(DeleteBehavior.Restrict);
    }
}
