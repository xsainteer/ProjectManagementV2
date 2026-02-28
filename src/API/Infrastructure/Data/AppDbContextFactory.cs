using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // too lazy to read connection string from config, just hardcoding it here for now
        const string connectionString = "Server=localhost,1433;Database=ProjectManagementV2;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=True";
        
        optionsBuilder.UseSqlServer(connectionString);
        
        return new AppDbContext(optionsBuilder.Options);
    }
}