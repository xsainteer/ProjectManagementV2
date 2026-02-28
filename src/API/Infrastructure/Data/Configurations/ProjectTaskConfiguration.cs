using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class ProjectTaskConfiguration : IEntityTypeConfiguration<ProjectTask>
{
    public void Configure(EntityTypeBuilder<ProjectTask> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Status).IsRequired().HasConversion<string>();
        builder.Property(t => t.Priority).IsRequired();
        builder.Property(t => t.Comment).HasMaxLength(1000);

        builder.HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Author)
            .WithMany(e => e.AuthoredTasks)
            .HasForeignKey(t => t.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Executor)
            .WithMany(e => e.ExecutedTasks)
            .HasForeignKey(t => t.ExecutorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
