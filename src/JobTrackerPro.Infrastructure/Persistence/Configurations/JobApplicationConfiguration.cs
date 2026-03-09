using JobTrackerPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobTrackerPro.Infrastructure.Persistence.Configurations;

public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.HasKey(j => j.Id);

        builder.Property(j => j.Title).IsRequired().HasMaxLength(200);
        builder.Property(j => j.JobUrl).HasMaxLength(500);
        builder.Property(j => j.Source).HasMaxLength(100);
        builder.Property(j => j.SalaryCurrency).HasMaxLength(10);

        // TechStack as owned entity (stored in same table)
        builder.OwnsOne(j => j.TechStack, ts =>
        {
            ts.Property(t => t.Backend).HasColumnName("TechStack_Backend").HasMaxLength(500);
            ts.Property(t => t.Frontend).HasColumnName("TechStack_Frontend").HasMaxLength(500);
            ts.Property(t => t.Databases).HasColumnName("TechStack_Databases").HasMaxLength(500);
            ts.Property(t => t.CloudAndDevOps).HasColumnName("TechStack_CloudAndDevOps").HasMaxLength(500);
            ts.Property(t => t.Testing).HasColumnName("TechStack_Testing").HasMaxLength(500);
        });

        builder.HasOne(j => j.Company)
            .WithMany(c => c.JobApplications)
            .HasForeignKey(j => j.CompanyId);
    }
}