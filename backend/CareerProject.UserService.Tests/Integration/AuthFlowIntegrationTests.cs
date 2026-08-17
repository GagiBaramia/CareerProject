using System.Net;
using System.Net.Http.Json;
using CareerProject.Shared.Data;
using CareerProject.Shared.Entities;
using CareerProject.UserService.Dtos;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CareerProject.UserService.Tests.Integration;

// Requires the real dev PostgreSQL (docker compose up -d) plus Jwt__Secret and POSTGRES_* env
// vars set (same as running the service normally) - boots the actual UserService pipeline
// in-process and drives it over real HTTP, hitting the real database.
public class AuthFlowIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly List<string> _emailsToCleanUp = [];

    public AuthFlowIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RegisterPerson_ThenLogin_Succeeds()
    {
        var client = _factory.CreateClient();
        var email = TrackEmail("person");

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register/person", new
        {
            email,
            password = "correct-password",
            fullName = "Auth Flow Test Person",
        });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var registered = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(registered);
        Assert.Equal("Person", registered!.Role);
        Assert.False(string.IsNullOrWhiteSpace(registered.Token));

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "correct-password" });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var loggedIn = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(loggedIn);
        Assert.Equal(registered.UserId, loggedIn!.UserId);
    }

    [Fact]
    public async Task RegisterCompany_AssignsCompanyRole()
    {
        var client = _factory.CreateClient();
        var email = TrackEmail("company");

        var response = await client.PostAsJsonAsync("/api/auth/register/company", new
        {
            email,
            password = "correct-password",
            companyName = "Auth Flow Test Co",
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.Equal("Company", body!.Role);
    }

    [Fact]
    public async Task RegisterPerson_DuplicateEmail_ReturnsConflict()
    {
        var client = _factory.CreateClient();
        var email = TrackEmail("duplicate");

        await client.PostAsJsonAsync("/api/auth/register/person", new
        {
            email,
            password = "correct-password",
            fullName = "First",
        });

        var secondAttempt = await client.PostAsJsonAsync("/api/auth/register/person", new
        {
            email,
            password = "another-password",
            fullName = "Second",
        });

        Assert.Equal(HttpStatusCode.Conflict, secondAttempt.StatusCode);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var email = TrackEmail("wrong-password");

        await client.PostAsJsonAsync("/api/auth/register/person", new
        {
            email,
            password = "correct-password",
            fullName = "Wrong Password Test",
        });

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "totally-wrong" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_UnknownEmail_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = $"no-such-user-{Guid.NewGuid()}@example.com",
            password = "whatever-password",
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private string TrackEmail(string label)
    {
        var email = $"auth-flow-test-{label}-{Guid.NewGuid()}@example.com";
        _emailsToCleanUp.Add(email);
        return email;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        var options = new DbContextOptionsBuilder<CareerProjectDbContext>()
            .UseNpgsql(PostgresConnectionStringBuilder.BuildFromEnvironment(), o => o.UseVector())
            .Options;

        await using var db = new CareerProjectDbContext(options);
        var users = await db.Users.Where(u => _emailsToCleanUp.Contains(u.Email)).ToListAsync();
        db.Users.RemoveRange(users);
        await db.SaveChangesAsync();
    }
}
