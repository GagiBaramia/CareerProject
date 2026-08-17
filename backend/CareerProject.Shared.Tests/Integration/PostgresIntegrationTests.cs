using CareerProject.Shared.Data;
using CareerProject.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CareerProject.Shared.Tests.Integration;

// Requires the real dev PostgreSQL (docker compose up -d, POSTGRES_* env vars set) - proves
// CareerProjectDbContext actually round-trips through a real database, not an in-memory fake.
public class PostgresIntegrationTests
{
    private static CareerProjectDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CareerProjectDbContext>()
            .UseNpgsql(PostgresConnectionStringBuilder.BuildFromEnvironment(), o => o.UseVector())
            .Options;

        return new CareerProjectDbContext(options);
    }

    [Fact]
    public async Task InsertedUser_IsReadableInANewDbContextInstance()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"pg-integration-test-{Guid.NewGuid()}@example.com",
            PasswordHash = "not-a-real-hash",
            Role = UserRole.Person,
            CreatedAt = DateTime.UtcNow,
        };

        await using (var writeDb = CreateContext())
        {
            writeDb.Users.Add(user);
            await writeDb.SaveChangesAsync();
        }

        try
        {
            await using var readDb = CreateContext();
            var reloaded = await readDb.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

            Assert.NotNull(reloaded);
            Assert.Equal(user.Email, reloaded!.Email);
            Assert.Equal(UserRole.Person, reloaded.Role);
        }
        finally
        {
            await using var cleanupDb = CreateContext();
            cleanupDb.Users.Remove(user);
            await cleanupDb.SaveChangesAsync();
        }
    }
}
