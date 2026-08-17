using System.Security.Claims;
using CareerProject.JobService.Auth;
using CareerProject.JobService.Config;
using CareerProject.JobService.Dtos;
using CareerProject.Shared.Data;
using CareerProject.Shared.Entities;
using CareerProject.Shared.Storage;
using CareerProject.Shared.Validation;
using Microsoft.Extensions.Options;

namespace CareerProject.JobService.Endpoints;

public static class CompanyEndpoints
{
    public static void MapCompanyEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/company")
            .WithTags("Company")
            .RequireAuthorization("CompanyOnly");

        group.MapGet("/me", GetMyCompany);
        group.MapPut("/me", UpdateMyCompany);
        group.MapPost("/me/logo", UploadLogo).DisableAntiforgery();
    }

    private static async Task<IResult> GetMyCompany(ClaimsPrincipal user, CareerProjectDbContext db)
    {
        var company = await CurrentUserResolver.LoadCurrentCompanyAsync(user, db);
        if (company is null)
            return Results.NotFound();

        return Results.Ok(ToResponse(company));
    }

    private static async Task<IResult> UpdateMyCompany(
        UpdateCompanyProfileRequest request,
        ClaimsPrincipal user,
        CareerProjectDbContext db)
    {
        if (!RequestValidator.TryValidate(request, out var errors))
            return Results.ValidationProblem(errors);

        var company = await CurrentUserResolver.LoadCurrentCompanyAsync(user, db);
        if (company is null)
            return Results.NotFound();

        company.Name = request.Name;
        company.Description = request.Description;
        company.Industry = request.Industry;
        company.LogoUrl = request.LogoUrl;

        await db.SaveChangesAsync();

        return Results.Ok(ToResponse(company));
    }

    private static async Task<IResult> UploadLogo(
        IFormFile file,
        ClaimsPrincipal user,
        CareerProjectDbContext db,
        IFileStorage storage,
        IOptions<UploadOptions> options,
        CancellationToken cancellationToken)
    {
        var company = await CurrentUserResolver.LoadCurrentCompanyAsync(user, db);
        if (company is null)
            return Results.NotFound();

        if (!ImageUploadValidator.TryValidate(file.ContentType, file.Length, options.Value.MaxImageBytes, out var extension, out var error))
            return Results.BadRequest(new { message = error });

        await using var stream = file.OpenReadStream();
        var url = await storage.SaveAsync(stream, extension, "logos", cancellationToken);

        company.LogoUrl = url;
        await db.SaveChangesAsync(cancellationToken);

        return Results.Ok(new { logoUrl = url });
    }

    private static CompanyProfileResponse ToResponse(Company company) => new()
    {
        Id = company.Id,
        Name = company.Name,
        Description = company.Description,
        Industry = company.Industry,
        LogoUrl = company.LogoUrl,
    };
}
