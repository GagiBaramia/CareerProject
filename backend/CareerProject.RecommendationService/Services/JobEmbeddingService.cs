using CareerProject.Shared.Data;
using CareerProject.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pgvector;

namespace CareerProject.RecommendationService.Services;

public class JobEmbeddingService(
    CareerProjectDbContext db,
    GeminiEmbeddingClient embeddingClient,
    ILogger<JobEmbeddingService> logger)
{
    public async Task RecomputeAsync(Guid jobId, CancellationToken cancellationToken)
    {
        var job = await db.Jobs
            .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
            .FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);

        if (job is null)
        {
            logger.LogWarning("Job {JobId} not found, skipping embedding.", jobId);
            return;
        }

        var text = BuildEmbeddingText(job);
        var values = await embeddingClient.EmbedAsync(text, "RETRIEVAL_DOCUMENT", cancellationToken);

        job.Embedding = new Vector(values);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Updated embedding for Job {JobId}.", jobId);
    }

    private static string BuildEmbeddingText(Job job)
    {
        var parts = new List<string> { job.Title, job.Description };

        if (job.JobSkills.Count > 0)
            parts.Add(string.Join(", ", job.JobSkills.Select(js => js.Skill.Name)));

        return string.Join(". ", parts);
    }
}
