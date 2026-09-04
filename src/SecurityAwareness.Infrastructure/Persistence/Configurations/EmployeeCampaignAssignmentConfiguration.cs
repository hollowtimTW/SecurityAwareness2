using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecurityAwareness.Infrastructure.Entities;

namespace SecurityAwareness.Infrastructure.Persistence.Configurations;

public class EmployeeCampaignAssignmentConfiguration : IEntityTypeConfiguration<EmployeeCampaignAssignment>
{
    public void Configure(EntityTypeBuilder<EmployeeCampaignAssignment> builder)
    {
        builder.ToTable("EmployeeCampaignAssignments");
        builder.HasKey(x => x.AssignmentId);
        builder.Property(x => x.AssignmentId).ValueGeneratedOnAdd();

        builder.Property(x => x.TrackingToken).HasMaxLength(36).IsRequired();
        builder.HasIndex(x => x.TrackingToken).IsUnique();
        builder.Property(x => x.TokenUrl).HasMaxLength(500);
        builder.Property(x => x.Status).IsRequired();

        builder.HasOne(x => x.Campaign)
            .WithMany(c => c.Assignments)
            .HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CampaignId, x.EmployeeId }).IsUnique();
    }
}
