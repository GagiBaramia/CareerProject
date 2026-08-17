using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CareerProject.JobService.Dtos;
using CareerProject.Shared.Data;
using CareerProject.Shared.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CareerProject.JobService.Tests.Integration;

// Requires the real dev PostgreSQL (docker compose up -d) plus Jwt__Secret and POSTGRES_* env
// vars set - boots the actual JobService pipeline in-process over real HTTP against the real
// database. Company/Person identities are seeded directly (JobService itself never creates
// Users - that's UserService's job in the real system; the two share one database).
public class JobCrudIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly List<Guid> _jobIdsToCleanUp = [];

    private Guid _ownerUserId;
    private Guid _ownerCompanyId;
    private Guid _otherUserId;
    private Guid _personUserId;

    private string _ownerToken = null!;
    private string _otherCompanyToken = null!;
    private string _personToken = null!;

    public JobCrudIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _ownerUserId = Guid.NewGuid();
        _ownerCompanyId = Guid.NewGuid();
        _otherUserId = Guid.NewGuid();
        var otherCompanyId = Guid.NewGuid();
        _personUserId = Guid.NewGuid();

        await using var db = CreateDbContext();

        db.Users.Add(new User { Id = _ownerUserId, Email = $"jobcrud-owner-{_ownerUserId}@example.com", PasswordHash = "n/a", Role = UserRole.Company, CreatedAt = DateTime.UtcNow });
        db.Companies.Add(new Company { Id = _ownerCompanyId, UserId = _ownerUserId, Name = "JobCrud Test Co" });

        db.Users.Add(new User { Id = _otherUserId, Email = $"jobcrud-other-{_otherUserId}@example.com", PasswordHash = "n/a", Role = UserRole.Company, CreatedAt = DateTime.UtcNow });
        db.Companies.Add(new Company { Id = otherCompanyId, UserId = _otherUserId, Name = "JobCrud Other Co" });

        db.Users.Add(new User { Id = _personUserId, Email = $"jobcrud-person-{_personUserId}@example.com", PasswordHash = "n/a", Role = UserRole.Person, CreatedAt = DateTime.UtcNow });
        db.PersonProfiles.Add(new PersonProfile { Id = Guid.NewGuid(), UserId = _personUserId, FullName = "JobCrud Test Person" });

        await db.SaveChangesAsync();

        _ownerToken = TestTokenFactory.CreateToken(_ownerUserId, "Company");
        _otherCompanyToken = TestTokenFactory.CreateToken(_otherUserId, "Company");
        _personToken = TestTokenFactory.CreateToken(_personUserId, "Person");
    }

    public async Task DisposeAsync()
    {
        await using var db = CreateDbContext();

        var jobs = await db.Jobs.Where(j => _jobIdsToCleanUp.Contains(j.Id)).ToListAsync();
        db.Jobs.RemoveRange(jobs);

        var users = await db.Users
            .Where(u => u.Id == _ownerUserId || u.Id == _otherUserId || u.Id == _personUserId)
            .ToListAsync();
        db.Users.RemoveRange(users); // cascades to Company/PersonProfile per DbContext config

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateJob_AsOwningCompany_Succeeds()
    {
        var client = CreateClient(_ownerToken);

        var response = await client.PostAsJsonAsync("/api/jobs", NewJobPayload());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var job = await response.Content.ReadFromJsonAsync<JobResponse>();
        Assert.NotNull(job);
        _jobIdsToCleanUp.Add(job!.Id);
        Assert.Equal(_ownerCompanyId, job.CompanyId);
    }

    [Fact]
    public async Task CreateJob_AsPerson_ReturnsForbidden()
    {
        var client = CreateClient(_personToken);

        var response = await client.PostAsJsonAsync("/api/jobs", NewJobPayload());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateJob_NoToken_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/jobs", NewJobPayload());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetJob_AfterCreate_ReturnsSameJob()
    {
        var client = CreateClient(_ownerToken);
        var created = await CreateJobAsync(client, "Get Test Job");

        var response = await client.GetAsync($"/api/jobs/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await response.Content.ReadFromJsonAsync<JobResponse>();
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal("Get Test Job", fetched.Title);
    }

    [Fact]
    public async Task ListJobs_IncludesCreatedJob()
    {
        var client = CreateClient(_ownerToken);
        var created = await CreateJobAsync(client, "List Test Job");

        var response = await client.GetAsync("/api/jobs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var jobs = await response.Content.ReadFromJsonAsync<List<JobResponse>>();
        Assert.Contains(jobs!, j => j.Id == created.Id);
    }

    [Fact]
    public async Task UpdateJob_AsOwner_Succeeds()
    {
        var client = CreateClient(_ownerToken);
        var created = await CreateJobAsync(client, "Update Test Job");

        var response = await client.PutAsJsonAsync($"/api/jobs/{created.Id}", NewJobPayload("Updated Title"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<JobResponse>();
        Assert.Equal("Updated Title", updated!.Title);
    }

    [Fact]
    public async Task UpdateJob_AsNonOwningCompany_ReturnsForbidden()
    {
        var ownerClient = CreateClient(_ownerToken);
        var created = await CreateJobAsync(ownerClient, "Ownership Test Job");

        var otherClient = CreateClient(_otherCompanyToken);
        var response = await otherClient.PutAsJsonAsync($"/api/jobs/{created.Id}", NewJobPayload("Hijacked Title"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteJob_AsOwner_RemovesJob()
    {
        var client = CreateClient(_ownerToken);
        var created = await CreateJobAsync(client, "Delete Test Job");

        var deleteResponse = await client.DeleteAsync($"/api/jobs/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/jobs/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteJob_AsNonOwningCompany_ReturnsForbidden()
    {
        var ownerClient = CreateClient(_ownerToken);
        var created = await CreateJobAsync(ownerClient, "Delete Ownership Test Job");

        var otherClient = CreateClient(_otherCompanyToken);
        var response = await otherClient.DeleteAsync($"/api/jobs/{created.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<JobResponse> CreateJobAsync(HttpClient client, string title)
    {
        var response = await client.PostAsJsonAsync("/api/jobs", NewJobPayload(title));
        var job = await response.Content.ReadFromJsonAsync<JobResponse>();
        _jobIdsToCleanUp.Add(job!.Id);
        return job;
    }

    private HttpClient CreateClient(string? token = null)
    {
        var client = _factory.CreateClient();
        if (token is not null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static CareerProjectDbContext CreateDbContext() => new(
        new DbContextOptionsBuilder<CareerProjectDbContext>()
            .UseNpgsql(PostgresConnectionStringBuilder.BuildFromEnvironment(), o => o.UseVector())
            .Options);

    private static object NewJobPayload(string title = "Integration Test Job") => new
    {
        title,
        description = "Created by an automated integration test.",
        employmentType = "FullTime",
        workFormat = "Remote",
        location = "Tbilisi",
        requiredSkills = Array.Empty<object>(),
    };
}
