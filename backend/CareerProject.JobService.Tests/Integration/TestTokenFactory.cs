using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CareerProject.JobService.Tests.Integration;

// Mirrors CareerProject.UserService's JwtTokenService so integration tests can mint tokens
// with the same claims shape JobService actually validates. Issuer/Audience match
// appsettings.json (not secret); the signing secret comes from Jwt__Secret, same as the
// real service - the docker-compose dev stack must be running with that env var set.
internal static class TestTokenFactory
{
    private const string Issuer = "CareerProject";
    private const string Audience = "CareerProject.Clients";

    public static string CreateToken(Guid userId, string role)
    {
        var secret = Environment.GetEnvironmentVariable("Jwt__Secret")
            ?? throw new InvalidOperationException("Jwt__Secret environment variable is not set.");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, $"{userId}@example.com"),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
