using CareerProject.Shared.Data;
using CareerProject.UserService.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CareerProject.UserService.Endpoints;

public static class SkillsEndpoints
{
    public static void MapSkillsEndpoints(this WebApplication app)
    {
        app.MapGet("/api/skills", GetSkills).WithTags("Skills");
    }

    private static async Task<IResult> GetSkills(string? search, CareerProjectDbContext db)
    {
        var query = db.Skills.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => EF.Functions.ILike(s.Name, $"%{search}%"));

        var skills = await query
            .OrderBy(s => s.Name)
            .Select(s => new SkillDto { Id = s.Id, Name = s.Name })
            .ToListAsync();

        return Results.Ok(skills);
    }
}
