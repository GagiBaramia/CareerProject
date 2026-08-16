using CareerProject.Shared.Data;
using CareerProject.Shared.Entities;
using CareerProject.UserService.Auth;
using CareerProject.UserService.Dtos;
using CareerProject.Shared.Validation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CareerProject.UserService.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register/person", RegisterPerson);
        group.MapPost("/register/company", RegisterCompany);
        group.MapPost("/login", Login);
    }

    private static async Task<IResult> RegisterPerson(
        RegisterPersonRequest request,
        CareerProjectDbContext db,
        JwtTokenService tokenService)
    {
        if (!RequestValidator.TryValidate(request, out var errors))
            return Results.ValidationProblem(errors);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == normalizedEmail))
            return Results.Conflict(new { message = "Email is already registered." });

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            Role = UserRole.Person,
            CreatedAt = DateTime.UtcNow,
        };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.Password);

        var profile = new PersonProfile
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            FullName = request.FullName,
        };

        db.Users.Add(user);
        db.PersonProfiles.Add(profile);
        await db.SaveChangesAsync();

        var token = tokenService.GenerateToken(user);
        return Results.Created($"/api/users/{user.Id}", new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.ToString(),
            DisplayName = profile.FullName,
        });
    }

    private static async Task<IResult> RegisterCompany(
        RegisterCompanyRequest request,
        CareerProjectDbContext db,
        JwtTokenService tokenService)
    {
        if (!RequestValidator.TryValidate(request, out var errors))
            return Results.ValidationProblem(errors);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == normalizedEmail))
            return Results.Conflict(new { message = "Email is already registered." });

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            Role = UserRole.Company,
            CreatedAt = DateTime.UtcNow,
        };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.Password);

        var company = new Company
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Name = request.CompanyName,
        };

        db.Users.Add(user);
        db.Companies.Add(company);
        await db.SaveChangesAsync();

        var token = tokenService.GenerateToken(user);
        return Results.Created($"/api/users/{user.Id}", new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.ToString(),
            DisplayName = company.Name,
        });
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        CareerProjectDbContext db,
        JwtTokenService tokenService)
    {
        if (!RequestValidator.TryValidate(request, out var errors))
            return Results.ValidationProblem(errors);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users
            .Include(u => u.PersonProfile)
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

        if (user is null)
            return Results.Unauthorized();

        var verification = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
            return Results.Unauthorized();

        var token = tokenService.GenerateToken(user);
        var displayName = user.Role == UserRole.Person
            ? user.PersonProfile?.FullName ?? user.Email
            : user.Company?.Name ?? user.Email;

        return Results.Ok(new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.ToString(),
            DisplayName = displayName,
        });
    }
}
