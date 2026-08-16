using System.Security.Claims;
using CareerProject.JobService.Auth;
using CareerProject.JobService.Dtos;
using CareerProject.Shared.Data;
using CareerProject.Shared.Entities;
using CareerProject.Shared.Validation;

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

    private static CompanyProfileResponse ToResponse(Company company) => new()
    {
        Name = company.Name,
        Description = company.Description,
        Industry = company.Industry,
        LogoUrl = company.LogoUrl,
    };
}
