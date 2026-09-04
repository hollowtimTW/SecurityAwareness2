using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAwareness.Infrastructure.Entities;

namespace SecurityAwareness.Infrastructure.Persistence.Configurations;

public class SenderMailboxConfiguration : IEntityTypeConfiguration<SenderMailbox>
{
    public void Configure(EntityTypeBuilder<SenderMailbox> builder)
    {
        builder.ToTable("SenderMailboxes");
        builder.HasKey(x => x.SenderMailboxId);
        builder.Property(x => x.SenderMailboxId).ValueGeneratedOnAdd();
        builder.Property(x => x.Email).HasMaxLength(120).IsRequired();
        builder.HasIndex(x => x.Email).IsUnique();
        builder.Property(x => x.DisplayName).HasMaxLength(60).IsRequired();
        builder.Property(x => x.Provider).IsRequired();
        builder.Property(x => x.DailyQuota).IsRequired();
        builder.Property(x => x.DailyQuotaUsed).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
    }
}
