using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAwareness.Infrastructure.Entities;

namespace SecurityAwareness.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(x => x.AuditLogId);
        builder.Property(x => x.AuditLogId).ValueGeneratedOnAdd();
        builder.Property(x => x.Action).HasMaxLength(50).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.EntityId).HasMaxLength(50);
        builder.Property(x => x.ActorUsername).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Detail).HasColumnType("nvarchar(max)");

        builder.HasIndex(x => new { x.ActorUsername, x.OccurredAt });
        builder.HasIndex(x => new { x.EntityType, x.EntityId, x.OccurredAt });
    }
}
