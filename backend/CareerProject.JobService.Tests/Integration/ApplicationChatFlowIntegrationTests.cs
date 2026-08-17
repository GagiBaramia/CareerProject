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

// Covers the full Apply -> Accept -> Conversation -> Messages chain against the real dev
// PostgreSQL (docker compose up -d, Jwt__Secret/POSTGRES_* env vars set) - the piece that
// actually proves "accepting an application opens exactly one private chat" end to end.
public class ApplicationChatFlowIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;

    private Guid _companyUserId;
    private Guid _companyId;
    private Guid _otherCompanyUserId;
    private Guid _personUserId;
    private Guid _jobId;

    private string _companyToken = null!;
    private string _otherCompanyToken = null!;
    private string _personToken = null!;

    public ApplicationChatFlowIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _companyUserId = Guid.NewGuid();
        _companyId = Guid.NewGuid();
        _otherCompanyUserId = Guid.NewGuid();
        var otherCompanyId = Guid.NewGuid();
        _personUserId = Guid.NewGuid();
        _jobId = Guid.NewGuid();

        await using var db = CreateDbContext();

        db.Users.Add(new User { Id = _companyUserId, Email = $"chatflow-company-{_companyUserId}@example.com", PasswordHash = "n/a", Role = UserRole.Company, CreatedAt = DateTime.UtcNow });
        db.Companies.Add(new Company { Id = _companyId, UserId = _companyUserId, Name = "ChatFlow Test Co" });

        db.Users.Add(new User { Id = _otherCompanyUserId, Email = $"chatflow-other-{_otherCompanyUserId}@example.com", PasswordHash = "n/a", Role = UserRole.Company, CreatedAt = DateTime.UtcNow });
        db.Companies.Add(new Company { Id = otherCompanyId, UserId = _otherCompanyUserId, Name = "ChatFlow Other Co" });

        db.Users.Add(new User { Id = _personUserId, Email = $"chatflow-person-{_personUserId}@example.com", PasswordHash = "n/a", Role = UserRole.Person, CreatedAt = DateTime.UtcNow });
        db.PersonProfiles.Add(new PersonProfile { Id = Guid.NewGuid(), UserId = _personUserId, FullName = "ChatFlow Test Person" });

        db.Jobs.Add(new Job
        {
            Id = _jobId,
            CompanyId = _companyId,
            Title = "ChatFlow Test Job",
            Description = "For integration testing.",
            EmploymentType = "FullTime",
            WorkFormat = "Remote",
            Location = "Tbilisi",
            CreatedAt = DateTime.UtcNow,
        });

        await db.SaveChangesAsync();

        _companyToken = TestTokenFactory.CreateToken(_companyUserId, "Company");
        _otherCompanyToken = TestTokenFactory.CreateToken(_otherCompanyUserId, "Company");
        _personToken = TestTokenFactory.CreateToken(_personUserId, "Person");
    }

    public async Task DisposeAsync()
    {
        await using var db = CreateDbContext();

        // Applications/Conversations/DirectMessages all cascade from these Users/the Job -
        // deleting the seeded Users and Job cleans up everything transitively.
        var users = await db.Users
            .Where(u => u.Id == _companyUserId || u.Id == _otherCompanyUserId || u.Id == _personUserId)
            .ToListAsync();
        db.Users.RemoveRange(users);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Apply_ThenDuplicateApply_SecondAttemptReturnsConflict()
    {
        var client = CreateClient(_personToken);

        var first = await client.PostAsync($"/api/jobs/{_jobId}/apply", null);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await client.PostAsync($"/api/jobs/{_jobId}/apply", null);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task OwningCompany_SeesApplication_OtherCompanyIsForbidden()
    {
        await CreateClient(_personToken).PostAsync($"/api/jobs/{_jobId}/apply", null);

        var ownerResponse = await CreateClient(_companyToken).GetAsync($"/api/company/jobs/{_jobId}/applications");
        Assert.Equal(HttpStatusCode.OK, ownerResponse.StatusCode);
        var applications = await ownerResponse.Content.ReadFromJsonAsync<List<ApplicationResponse>>();
        Assert.Single(applications!);

        var otherResponse = await CreateClient(_otherCompanyToken).GetAsync($"/api/company/jobs/{_jobId}/applications");
        Assert.Equal(HttpStatusCode.Forbidden, otherResponse.StatusCode);
    }

    [Fact]
    public async Task AcceptingApplication_CreatesExactlyOneConversation_EvenIfAcceptedTwice()
    {
        var applyResponse = await CreateClient(_personToken).PostAsync($"/api/jobs/{_jobId}/apply", null);
        var application = await applyResponse.Content.ReadFromJsonAsync<ApplicationResponse>();

        var companyClient = CreateClient(_companyToken);

        var firstAccept = await companyClient.PatchAsJsonAsync($"/api/applications/{application!.Id}/status", new { status = "Accepted" });
        Assert.Equal(HttpStatusCode.OK, firstAccept.StatusCode);

        var secondAccept = await companyClient.PatchAsJsonAsync($"/api/applications/{application.Id}/status", new { status = "Accepted" });
        Assert.Equal(HttpStatusCode.OK, secondAccept.StatusCode);

        await using var db = CreateDbContext();
        var conversationCount = await db.Conversations.CountAsync(c => c.ApplicationId == application.Id);
        Assert.Equal(1, conversationCount);
    }

    [Fact]
    public async Task AfterAccepted_ParticipantsCanExchangeMessages_OutsidersCannotRead()
    {
        var applyResponse = await CreateClient(_personToken).PostAsync($"/api/jobs/{_jobId}/apply", null);
        var application = await applyResponse.Content.ReadFromJsonAsync<ApplicationResponse>();

        var companyClient = CreateClient(_companyToken);
        await companyClient.PatchAsJsonAsync($"/api/applications/{application!.Id}/status", new { status = "Accepted" });

        await using var db = CreateDbContext();
        var conversation = await db.Conversations.FirstAsync(c => c.ApplicationId == application.Id);

        var sendResponse = await companyClient.PostAsJsonAsync($"/api/conversations/{conversation.Id}/messages", new { content = "მოგესალმებით!" });
        Assert.Equal(HttpStatusCode.Created, sendResponse.StatusCode);

        var candidateClient = CreateClient(_personToken);
        var messagesResponse = await candidateClient.GetAsync($"/api/conversations/{conversation.Id}/messages");
        Assert.Equal(HttpStatusCode.OK, messagesResponse.StatusCode);
        var messages = await messagesResponse.Content.ReadFromJsonAsync<List<DirectMessageResponse>>();
        Assert.Single(messages!);
        Assert.Equal("მოგესალმებით!", messages![0].Content);

        var outsiderResponse = await CreateClient(_otherCompanyToken).GetAsync($"/api/conversations/{conversation.Id}/messages");
        Assert.Equal(HttpStatusCode.Forbidden, outsiderResponse.StatusCode);
    }

    [Fact]
    public async Task RejectedApplication_DoesNotCreateConversation()
    {
        var applyResponse = await CreateClient(_personToken).PostAsync($"/api/jobs/{_jobId}/apply", null);
        var application = await applyResponse.Content.ReadFromJsonAsync<ApplicationResponse>();

        await CreateClient(_companyToken).PatchAsJsonAsync($"/api/applications/{application!.Id}/status", new { status = "Rejected" });

        await using var db = CreateDbContext();
        var conversationExists = await db.Conversations.AnyAsync(c => c.ApplicationId == application.Id);
        Assert.False(conversationExists);
    }

    private HttpClient CreateClient(string token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static CareerProjectDbContext CreateDbContext() => new(
        new DbContextOptionsBuilder<CareerProjectDbContext>()
            .UseNpgsql(PostgresConnectionStringBuilder.BuildFromEnvironment(), o => o.UseVector())
            .Options);
}
