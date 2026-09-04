using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAwareness.Infrastructure.Entities;

namespace SecurityAwareness.Infrastructure.Persistence.Configurations;

public class CampaignScheduleConfiguration : IEntityTypeConfiguration<CampaignSchedule>
{
    public void Configure(EntityTypeBuilder<CampaignSchedule> builder)
    {
        builder.ToTable("CampaignSchedules");
        builder.HasKey(x => x.ScheduleId);
        builder.Property(x => x.ScheduleId).ValueGeneratedOnAdd();
        builder.Property(x => x.ScheduleType).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(50).IsRequired();

        builder.HasOne(x => x.Campaign)
            .WithMany()
            .HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.NextRunAt);
    }
}
