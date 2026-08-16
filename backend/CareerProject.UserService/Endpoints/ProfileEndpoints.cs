using System.Security.Claims;
using CareerProject.Shared.Data;
using CareerProject.Shared.Entities;
using CareerProject.Shared.Events;
using CareerProject.Shared.Messaging;
using CareerProject.UserService.Dtos;
using CareerProject.Shared.Validation;
using Microsoft.EntityFrameworkCore;

namespace CareerProject.UserService.Endpoints;

public static class ProfileEndpoints
{
    public static void MapProfileEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/profile")
            .WithTags("Profile")
            .RequireAuthorization("PersonOnly");

        group.MapGet("/me", GetMyProfile);
        group.MapPut("/me", UpdateMyProfile);
        group.MapPut("/me/skills", UpdateMySkills);
    }

    private static async Task<IResult> GetMyProfile(ClaimsPrincipal user, CareerProjectDbContext db)
    {
        var profile = await LoadProfile(user, db);
        if (profile is null)
            return Results.NotFound();

        return Results.Ok(ToResponse(profile));
    }

    private static async Task<IResult> UpdateMyProfile(
        UpdateProfileRequest request,
        ClaimsPrincipal user,
        CareerProjectDbContext db,
        IEventPublisher publisher)
    {
        if (!RequestValidator.TryValidate(request, out var errors))
            return Results.ValidationProblem(errors);

        var profile = await LoadProfile(user, db);
        if (profile is null)
            return Results.NotFound();

        // A profile created at registration only has FullName set; treat the
        // first time Headline is filled in as "ProfileCreated" (the profile
        // becoming meaningful), and every edit after that as "ProfileUpdated".
        var isFirstCompletion = profile.Headline is null;

        profile.FullName = request.FullName;
        profile.Headline = request.Headline;
        profile.CvSummary = request.CvSummary;
        profile.Location = request.Location;

        await db.SaveChangesAsync();

        if (isFirstCompletion)
            await publisher.PublishAsync(new ProfileCreated { EntityId = profile.Id });
        else
            await publisher.PublishAsync(new ProfileUpdated { EntityId = profile.Id });

        return Results.Ok(ToResponse(profile));
    }

    private static async Task<IResult> UpdateMySkills(
        UpdateProfileSkillsRequest request,
        ClaimsPrincipal user,
        CareerProjectDbContext db,
        IEventPublisher publisher)
    {
        var profile = await LoadProfile(user, db);
        if (profile is null)
            return Results.NotFound();

        var parsedSkills = new List<(Guid SkillId, ProficiencyLevel Level)>();
        foreach (var input in request.Skills)
        {
            if (!Enum.TryParse<ProficiencyLevel>(input.Level, ignoreCase: true, out var level))
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["skills"] = [$"'{input.Level}' is not a valid proficiency level. Use Beginner, Intermediate, Advanced, or Expert."],
                });

            parsedSkills.Add((input.SkillId, level));
        }

        var skillIds = parsedSkills.Select(s => s.SkillId).ToList();
        var existingSkillIds = await db.Skills
            .Where(s => skillIds.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync();

        var unknownSkillIds = skillIds.Except(existingSkillIds).ToList();
        if (unknownSkillIds.Count > 0)
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["skills"] = [$"Unknown skill id(s): {string.Join(", ", unknownSkillIds)}"],
            });

        db.PersonSkills.RemoveRange(profile.PersonSkills);
        foreach (var (skillId, level) in parsedSkills)
        {
            profile.PersonSkills.Add(new PersonSkill
            {
                PersonId = profile.Id,
                SkillId = skillId,
                Level = level,
            });
        }

        await db.SaveChangesAsync();
        await publisher.PublishAsync(new ProfileUpdated { EntityId = profile.Id });

        var refreshed = await LoadProfile(user, db);
        return Results.Ok(ToResponse(refreshed!));
    }

    private static async Task<PersonProfile?> LoadProfile(ClaimsPrincipal user, CareerProjectDbContext db)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub")!);

        return await db.PersonProfiles
            .Include(p => p.PersonSkills)
                .ThenInclude(ps => ps.Skill)
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    private static ProfileResponse ToResponse(PersonProfile profile) => new()
    {
        FullName = profile.FullName,
        Headline = profile.Headline,
        CvSummary = profile.CvSummary,
        Location = profile.Location,
        Skills = profile.PersonSkills.Select(ps => new ProfileSkillDto
        {
            SkillId = ps.SkillId,
            SkillName = ps.Skill.Name,
            Level = ps.Level.ToString(),
        }).ToList(),
    };
}
