using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAwareness.Infrastructure.Entities;

namespace SecurityAwareness.Infrastructure.Persistence.Configurations;

public class EmailDeliveryLogConfiguration : IEntityTypeConfiguration<EmailDeliveryLog>
{
    public void Configure(EntityTypeBuilder<EmailDeliveryLog> builder)
    {
        builder.ToTable("EmailDeliveryLogs");
        builder.HasKey(x => x.LogId);
        builder.Property(x => x.LogId).ValueGeneratedOnAdd();
        builder.Property(x => x.Subject).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.ProviderMessageId).HasMaxLength(200);
        builder.Property(x => x.ErrorDetail).HasMaxLength(1000);
        builder.Property(x => x.AttemptCount).IsRequired();

        builder.HasIndex(x => new { x.AssignmentId, x.CreatedAt });
    }
}
