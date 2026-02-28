using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.CustomerCompany).IsRequired().HasMaxLength(200);
        builder.Property(p => p.PerformerCompany).IsRequired().HasMaxLength(200);
        builder.Property(p => p.StartDate).IsRequired();
        builder.Property(p => p.Priority).IsRequired();

        builder.HasOne(p => p.ProjectManager)
            .WithMany(e => e.ManagedProjects)
            .HasForeignKey(p => p.ProjectManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Employees)
            .WithMany(e => e.Projects);

        builder.HasMany(p => p.Tasks)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Documents)
            .WithOne(d => d.Project)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
