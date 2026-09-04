using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAwareness.Infrastructure.Entities;

namespace SecurityAwareness.Infrastructure.Persistence.Configurations;

public class PhishingLinkClickConfiguration : IEntityTypeConfiguration<PhishingLinkClick>
{
    public void Configure(EntityTypeBuilder<PhishingLinkClick> builder)
    {
        builder.ToTable("PhishingLinkClicks");
        builder.HasKey(x => x.ClickId);
        builder.Property(x => x.ClickId).ValueGeneratedOnAdd();

        builder.Property(x => x.TrackingToken).HasMaxLength(36).IsRequired();
        builder.Property(x => x.IpAddress).HasMaxLength(45);
        builder.Property(x => x.UserAgent).HasMaxLength(500);
        builder.Property(x => x.LinkType).IsRequired();

        builder.HasOne(x => x.Assignment)
            .WithMany(a => a.Clicks)
            .HasForeignKey(x => x.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.AssignmentId);
        builder.HasIndex(x => x.ClickedAt);
    }
}
