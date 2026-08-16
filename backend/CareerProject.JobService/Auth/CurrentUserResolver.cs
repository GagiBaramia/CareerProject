using System.Security.Claims;
using CareerProject.Shared.Data;
using CareerProject.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace CareerProject.JobService.Auth;

public static class CurrentUserResolver
{
    public static Guid GetUserId(ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub")!);

    public static Task<Company?> LoadCurrentCompanyAsync(ClaimsPrincipal user, CareerProjectDbContext db) =>
        db.Companies.FirstOrDefaultAsync(c => c.UserId == GetUserId(user));

    public static Task<PersonProfile?> LoadCurrentPersonProfileAsync(ClaimsPrincipal user, CareerProjectDbContext db) =>
        db.PersonProfiles.FirstOrDefaultAsync(p => p.UserId == GetUserId(user));
}
