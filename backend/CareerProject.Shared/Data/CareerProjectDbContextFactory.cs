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
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
        var database = Environment.GetEnvironmentVariable("POSTGRES_DB")
            ?? throw new InvalidOperationException("POSTGRES_DB environment variable is required to run migrations.");
        var username = Environment.GetEnvironmentVariable("POSTGRES_USER")
            ?? throw new InvalidOperationException("POSTGRES_USER environment variable is required to run migrations.");
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")
            ?? throw new InvalidOperationException("POSTGRES_PASSWORD environment variable is required to run migrations.");

        var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";

        var optionsBuilder = new DbContextOptionsBuilder<CareerProjectDbContext>();
        optionsBuilder.UseNpgsql(connectionString, o => o.UseVector());

        return new CareerProjectDbContext(optionsBuilder.Options);
    }
}
