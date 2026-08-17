using System.Security.Claims;
using CareerProject.RecommendationService.Caching;
using CareerProject.RecommendationService.Config;
using CareerProject.RecommendationService.Dtos;
using CareerProject.RecommendationService.Services;
using CareerProject.Shared.Data;
using CareerProject.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace CareerProject.RecommendationService.Endpoints;

public static class RecommendationEndpoints
{
    public static void MapRecommendationEndpoints(this WebApplication app)
    {
        app.MapGroup("/api/recommendations")
            .WithTags("Recommendations")
            .RequireAuthorization("PersonOnly")
            .MapGet("/jobs", GetRecommendedJobs);
    }

    private static async Task<IResult> GetRecommendedJobs(
        ClaimsPrincipal user,
        CareerProjectDbContext db,
        IOptions<RecommendationOptions> options,
        IRecommendationCache cache,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub")!);

        var profile = await db.PersonProfiles
            .Include(p => p.PersonSkills)
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile is null)
            return Results.NotFound(new { message = "Person profile not found." });

        var cached = await cache.GetAsync(profile.Id, cancellationToken);
        if (cached is not null)
            return Results.Ok(cached);

        var personSkillIds = profile.PersonSkills.Select(ps => ps.SkillId).ToHashSet();
        var jobsWithSimilarity = await LoadJobsWithSimilarity(db, profile.Embedding);

        var weights = options.Value;
        var results = jobsWithSimilarity
            .Select(x =>
            {
                var requiredSkillIds = x.Job.JobSkills.Select(js => js.SkillId).ToHashSet();
                var skillOverlap = HybridMatchingCalculator.CalculateSkillOverlap(personSkillIds, requiredSkillIds);
                var score = HybridMatchingCalculator.CalculateScore(
                    skillOverlap, x.SemanticSimilarity, weights.StructuredWeight, weights.SemanticWeight);

                return ToResponse(x.Job, score, skillOverlap, x.SemanticSimilarity);
            })
            .OrderByDescending(r => r.Score)
            .ToList();

        await cache.SetAsync(profile.Id, results, cancellationToken);

        return Results.Ok(results);
    }

    private static async Task<List<(Job Job, double SemanticSimilarity)>> LoadJobsWithSimilarity(
        CareerProjectDbContext db, Vector? personEmbedding)
    {
        var query = db.Jobs
            .Include(j => j.Company)
            .Include(j => j.JobSkills).ThenInclude(js => js.Skill);

        if (personEmbedding is null)
        {
            // No embedding yet (profile summary not filled in) - rank on skill overlap alone.
            var jobs = await query.ToListAsync();
            return jobs.Select(j => (j, 0.0)).ToList();
        }

        var projected = await query
            .Select(j => new
            {
                Job = j,
                SemanticSimilarity = j.Embedding == null
                    ? 0.0
                    : 1.0 - j.Embedding.CosineDistance(personEmbedding)
            })
            .ToListAsync();

        return projected.Select(x => (x.Job, x.SemanticSimilarity)).ToList();
    }

    private static JobRecommendationResponse ToResponse(Job job, double score, double skillOverlap, double semanticSimilarity) => new()
    {
        JobId = job.Id,
        Title = job.Title,
        Description = job.Description,
        EmploymentType = job.EmploymentType,
        WorkFormat = job.WorkFormat,
        Location = job.Location,
        SalaryMin = job.SalaryMin,
        SalaryMax = job.SalaryMax,
        SalaryCurrency = job.SalaryCurrency,
        CompanyId = job.CompanyId,
        CompanyName = job.Company.Name,
        Score = score,
        SkillOverlap = skillOverlap,
        SemanticSimilarity = semanticSimilarity,
        RequiredSkills = job.JobSkills.Select(js => new RecommendationSkillDto
        {
            SkillId = js.SkillId,
            SkillName = js.Skill.Name,
            RequiredLevel = js.RequiredLevel.ToString(),
        }).ToList(),
    };
}
