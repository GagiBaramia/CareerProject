using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CareerProject.Shared.Data;

// Enables `dotnet ef migrations` to run against CareerProject.Shared directly,
// without a startup project. Connection info comes from environment variables
// only - never hardcode credentials here.
public class CareerProjectDbContextFactory : IDesignTimeDbContextFactory<CareerProjectDbContext>
{
    public CareerProjectDbContext CreateDbContext(string[] args)
    {
        var connectionString = PostgresConnectionStringBuilder.BuildFromEnvironment();

        var optionsBuilder = new DbContextOptionsBuilder<CareerProjectDbContext>();
        optionsBuilder.UseNpgsql(connectionString, o => o.UseVector());

        return new CareerProjectDbContext(optionsBuilder.Options);
    }
}
