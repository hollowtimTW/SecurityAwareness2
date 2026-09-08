using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAwareness.Infrastructure.Entities;

namespace SecurityAwareness.Infrastructure.Persistence.Configurations;

public class PhishingCampaignConfiguration : IEntityTypeConfiguration<PhishingCampaign>
{
    public void Configure(EntityTypeBuilder<PhishingCampaign> builder)
    {
        builder.ToTable("PhishingCampaigns");
        builder.HasKey(x => x.CampaignId);
        builder.Property(x => x.CampaignId).ValueGeneratedOnAdd();
        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Property(x => x.Title).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(50).IsRequired();
        builder.Property(x => x.TargetDepartmentIds).HasMaxLength(500);
    }
}
