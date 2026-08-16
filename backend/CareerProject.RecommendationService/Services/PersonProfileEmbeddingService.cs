using CareerProject.Shared.Data;
using CareerProject.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pgvector;

namespace CareerProject.RecommendationService.Services;

public class PersonProfileEmbeddingService(
    CareerProjectDbContext db,
    GeminiEmbeddingClient embeddingClient,
    ILogger<PersonProfileEmbeddingService> logger)
{
    public async Task RecomputeAsync(Guid personProfileId, CancellationToken cancellationToken)
    {
        var profile = await db.PersonProfiles
            .Include(p => p.PersonSkills).ThenInclude(ps => ps.Skill)
            .FirstOrDefaultAsync(p => p.Id == personProfileId, cancellationToken);

        if (profile is null)
        {
            logger.LogWarning("PersonProfile {PersonProfileId} not found, skipping embedding.", personProfileId);
            return;
        }

        var text = BuildEmbeddingText(profile);
        if (string.IsNullOrWhiteSpace(text))
        {
            logger.LogInformation("PersonProfile {PersonProfileId} has no embeddable content yet, skipping.", personProfileId);
            return;
        }

        var values = await embeddingClient.EmbedAsync(text, "RETRIEVAL_DOCUMENT", cancellationToken);

        profile.Embedding = new Vector(values);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Updated embedding for PersonProfile {PersonProfileId}.", personProfileId);
    }

    private static string BuildEmbeddingText(PersonProfile profile)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(profile.Headline))
            parts.Add(profile.Headline);

        if (!string.IsNullOrWhiteSpace(profile.CvSummary))
            parts.Add(profile.CvSummary);

        if (profile.PersonSkills.Count > 0)
            parts.Add(string.Join(", ", profile.PersonSkills.Select(ps => ps.Skill.Name)));

        return string.Join(". ", parts);
    }
}
