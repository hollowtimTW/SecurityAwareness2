using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAwareness.Infrastructure.Entities;

namespace SecurityAwareness.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(x => x.EmployeeId);
        builder.Property(x => x.EmployeeId).ValueGeneratedOnAdd();

        builder.Property(x => x.EmployeeNo).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => x.EmployeeNo).IsUnique();

        builder.Property(x => x.Email).HasMaxLength(120).IsRequired();
        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.DisplayName).HasMaxLength(60).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();

        builder.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
