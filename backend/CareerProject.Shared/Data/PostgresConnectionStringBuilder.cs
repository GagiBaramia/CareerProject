namespace CareerProject.Shared.Data;

// Builds the PostgreSQL connection string from environment variables only -
// mirrors the POSTGRES_* variables in .env, never hardcodes credentials.
public static class PostgresConnectionStringBuilder {
    public static string BuildFromEnvironment() {
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
        var database = Environment.GetEnvironmentVariable("POSTGRES_DB")
            ?? throw new InvalidOperationException("POSTGRES_DB environment variable is not set.");
        var username = Environment.GetEnvironmentVariable("POSTGRES_USER")
            ?? throw new InvalidOperationException("POSTGRES_USER environment variable is not set.");
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")
            ?? throw new InvalidOperationException("POSTGRES_PASSWORD environment variable is not set.");

        return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    }
}
