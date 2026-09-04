using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAwareness.Infrastructure.Entities;

namespace SecurityAwareness.Infrastructure.Persistence.Configurations;

public class SystemAccountConfiguration : IEntityTypeConfiguration<SystemAccount>
{
    public void Configure(EntityTypeBuilder<SystemAccount> builder)
    {
        builder.ToTable("SystemAccounts");
        builder.HasKey(x => x.AccountId);
        builder.Property(x => x.AccountId).ValueGeneratedOnAdd();
        builder.Property(x => x.Username).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.Username).IsUnique();
        builder.Property(x => x.PasswordHash).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(60).IsRequired();
        builder.Property(x => x.Role).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
    }
}
