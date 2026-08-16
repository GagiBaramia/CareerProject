using System.Security.Claims;
using CareerProject.JobService.Auth;
using CareerProject.JobService.Dtos;
using CareerProject.Shared.Data;
using CareerProject.Shared.Entities;
using CareerProject.Shared.Events;
using CareerProject.Shared.Messaging;
using CareerProject.Shared.Validation;
using Microsoft.EntityFrameworkCore;

namespace CareerProject.JobService.Endpoints;

public static class JobEndpoints
{
    public static void MapJobEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/jobs").WithTags("Jobs").RequireAuthorization();

        group.MapPost("/", CreateJob).RequireAuthorization("CompanyOnly");
        group.MapGet("/", ListJobs);
        group.MapGet("/{id:guid}", GetJob);
        group.MapPut("/{id:guid}", UpdateJob).RequireAuthorization("CompanyOnly");
        group.MapDelete("/{id:guid}", DeleteJob).RequireAuthorization("CompanyOnly");
    }

    private static async Task<IResult> CreateJob(
        JobRequest request,
        ClaimsPrincipal user,
        CareerProjectDbContext db,
        IEventPublisher publisher)
    {
        if (!RequestValidator.TryValidate(request, out var errors))
            return Results.ValidationProblem(errors);

        var company = await CurrentUserResolver.LoadCurrentCompanyAsync(user, db);
        if (company is null)
            return Results.NotFound(new { message = "Company profile not found." });

        if (!TryParseRequiredSkills(request.RequiredSkills, out var parsedSkills, out var skillErrors))
            return Results.ValidationProblem(skillErrors);

        var skillExistenceErrors = await ValidateSkillsExist(db, parsedSkills);
        if (skillExistenceErrors is not null)
            return Results.ValidationProblem(skillExistenceErrors);

        var job = new Job
        {
            Id = Guid.NewGuid(),
            CompanyId = company.Id,
            Title = request.Title,
            Description = request.Description,
            EmploymentType = request.EmploymentType,
            WorkFormat = request.WorkFormat,
            Location = request.Location,
            SalaryMin = request.SalaryMin,
            SalaryMax = request.SalaryMax,
            SalaryCurrency = request.SalaryCurrency,
            CreatedAt = DateTime.UtcNow,
        };

        foreach (var (skillId, level) in parsedSkills)
        {
            job.JobSkills.Add(new JobSkill { JobId = job.Id, SkillId = skillId, RequiredLevel = level });
        }

        db.Jobs.Add(job);
        await db.SaveChangesAsync();
        await publisher.PublishAsync(new JobCreated { EntityId = job.Id });

        var created = await LoadJob(db, job.Id);
        return Results.Created($"/api/jobs/{job.Id}", ToResponse(created!));
    }

    private static async Task<IResult> ListJobs(CareerProjectDbContext db)
    {
        var jobs = await db.Jobs
            .Include(j => j.Company)
            .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();

        return Results.Ok(jobs.Select(ToResponse));
    }

    private static async Task<IResult> GetJob(Guid id, CareerProjectDbContext db)
    {
        var job = await LoadJob(db, id);
        if (job is null)
            return Results.NotFound();

        return Results.Ok(ToResponse(job));
    }

    private static async Task<IResult> UpdateJob(
        Guid id,
        JobRequest request,
        ClaimsPrincipal user,
        CareerProjectDbContext db,
        IEventPublisher publisher)
    {
        if (!RequestValidator.TryValidate(request, out var errors))
            return Results.ValidationProblem(errors);

        var company = await CurrentUserResolver.LoadCurrentCompanyAsync(user, db);
        if (company is null)
            return Results.NotFound(new { message = "Company profile not found." });

        var job = await LoadJob(db, id);
        if (job is null)
            return Results.NotFound();

        if (job.CompanyId != company.Id)
            return Results.Forbid();

        if (!TryParseRequiredSkills(request.RequiredSkills, out var parsedSkills, out var skillErrors))
            return Results.ValidationProblem(skillErrors);

        var skillExistenceErrors = await ValidateSkillsExist(db, parsedSkills);
        if (skillExistenceErrors is not null)
            return Results.ValidationProblem(skillExistenceErrors);

        job.Title = request.Title;
        job.Description = request.Description;
        job.EmploymentType = request.EmploymentType;
        job.WorkFormat = request.WorkFormat;
        job.Location = request.Location;
        job.SalaryMin = request.SalaryMin;
        job.SalaryMax = request.SalaryMax;
        job.SalaryCurrency = request.SalaryCurrency;

        db.JobSkills.RemoveRange(job.JobSkills);
        job.JobSkills.Clear();
        foreach (var (skillId, level) in parsedSkills)
        {
            job.JobSkills.Add(new JobSkill { JobId = job.Id, SkillId = skillId, RequiredLevel = level });
        }

        await db.SaveChangesAsync();
        await publisher.PublishAsync(new JobUpdated { EntityId = job.Id });

        var updated = await LoadJob(db, job.Id);
        return Results.Ok(ToResponse(updated!));
    }

    private static async Task<IResult> DeleteJob(Guid id, ClaimsPrincipal user, CareerProjectDbContext db)
    {
        var company = await CurrentUserResolver.LoadCurrentCompanyAsync(user, db);
        if (company is null)
            return Results.NotFound(new { message = "Company profile not found." });

        var job = await db.Jobs.FirstOrDefaultAsync(j => j.Id == id);
        if (job is null)
            return Results.NotFound();

        if (job.CompanyId != company.Id)
            return Results.Forbid();

        db.Jobs.Remove(job);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    private static async Task<Job?> LoadJob(CareerProjectDbContext db, Guid id) =>
        await db.Jobs
            .Include(j => j.Company)
            .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
            .FirstOrDefaultAsync(j => j.Id == id);

    private static bool TryParseRequiredSkills(
        List<JobSkillInput> input,
        out List<(Guid SkillId, ProficiencyLevel Level)> parsed,
        out IDictionary<string, string[]> errors)
    {
        parsed = [];
        foreach (var item in input)
        {
            if (!Enum.TryParse<ProficiencyLevel>(item.RequiredLevel, ignoreCase: true, out var level))
            {
                errors = new Dictionary<string, string[]>
                {
                    ["requiredSkills"] = [$"'{item.RequiredLevel}' is not a valid proficiency level. Use Beginner, Intermediate, Advanced, or Expert."],
                };
                return false;
            }

            parsed.Add((item.SkillId, level));
        }

        errors = new Dictionary<string, string[]>();
        return true;
    }

    private static async Task<IDictionary<string, string[]>?> ValidateSkillsExist(
        CareerProjectDbContext db,
        List<(Guid SkillId, ProficiencyLevel Level)> parsedSkills)
    {
        var skillIds = parsedSkills.Select(s => s.SkillId).ToList();
        var existingSkillIds = await db.Skills.Where(s => skillIds.Contains(s.Id)).Select(s => s.Id).ToListAsync();
        var unknownSkillIds = skillIds.Except(existingSkillIds).ToList();

        if (unknownSkillIds.Count > 0)
        {
            return new Dictionary<string, string[]>
            {
                ["requiredSkills"] = [$"Unknown skill id(s): {string.Join(", ", unknownSkillIds)}"],
            };
        }

        return null;
    }

    private static JobResponse ToResponse(Job job) => new()
    {
        Id = job.Id,
        Title = job.Title,
        Description = job.Description,
        EmploymentType = job.EmploymentType,
        WorkFormat = job.WorkFormat,
        Location = job.Location,
        SalaryMin = job.SalaryMin,
        SalaryMax = job.SalaryMax,
        SalaryCurrency = job.SalaryCurrency,
        CreatedAt = job.CreatedAt,
        CompanyId = job.CompanyId,
        CompanyName = job.Company.Name,
        RequiredSkills = job.JobSkills.Select(js => new JobSkillDto
        {
            SkillId = js.SkillId,
            SkillName = js.Skill.Name,
            RequiredLevel = js.RequiredLevel.ToString(),
        }).ToList(),
    };
}
