using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.MiddleName).HasMaxLength(100);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(255);

        builder.HasMany(e => e.AuthoredTasks)
            .WithOne(t => t.Author)
            .HasForeignKey(t => t.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ExecutedTasks)
            .WithOne(t => t.Executor)
            .HasForeignKey(t => t.ExecutorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ManagedProjects)
            .WithOne(p => p.ProjectManager)
            .HasForeignKey(p => p.ProjectManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
