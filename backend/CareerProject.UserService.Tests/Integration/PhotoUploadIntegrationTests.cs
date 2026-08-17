using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CareerProject.Shared.Data;
using CareerProject.UserService.Dtos;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CareerProject.UserService.Tests.Integration;

// Requires the real dev PostgreSQL (docker compose up -d). Covers the endpoint-level MIME/size
// gate for /api/profile/me/photo - ImageUploadValidatorTests already covers the pure validation
// logic in isolation; this proves the endpoint actually wires that logic in and rejects/accepts
// the same way over real HTTP.
public class PhotoUploadIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private string _email = null!;
    private string _token = null!;

    public PhotoUploadIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _email = $"photo-upload-test-{Guid.NewGuid()}@example.com";
        var client = _factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register/person", new
        {
            email = _email,
            password = "correct-password",
            fullName = "Photo Upload Test Person",
        });

        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        _token = auth!.Token;
    }

    [Fact]
    public async Task UploadPhoto_NonImageContentType_ReturnsBadRequest()
    {
        var client = CreateAuthedClient();

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([1, 2, 3, 4]);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "file", "not-an-image.pdf");

        var response = await client.PostAsync("/api/profile/me/photo", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UploadPhoto_ValidPngImage_ReturnsPhotoUrl()
    {
        var client = CreateAuthedClient();

        // Smallest possible valid PNG (1x1 transparent pixel).
        var pngBytes = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(pngBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "photo.png");

        var response = await client.PostAsync("/api/profile/me/photo", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.StartsWith("/uploads/photos/", body!["photoUrl"]);
    }

    [Fact]
    public async Task UploadPhoto_NoToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([1, 2, 3]);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "photo.png");

        var response = await client.PostAsync("/api/profile/me/photo", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private HttpClient CreateAuthedClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        return client;
    }

    public async Task DisposeAsync()
    {
        var options = new DbContextOptionsBuilder<CareerProjectDbContext>()
            .UseNpgsql(PostgresConnectionStringBuilder.BuildFromEnvironment(), o => o.UseVector())
            .Options;

        await using var db = new CareerProjectDbContext(options);
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == _email);
        if (user is not null)
        {
            db.Users.Remove(user);
            await db.SaveChangesAsync();
        }
    }
}
