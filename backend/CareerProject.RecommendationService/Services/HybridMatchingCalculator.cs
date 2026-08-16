namespace CareerProject.RecommendationService.Services;

// Pure, DB-free scoring logic so it can be unit tested without EF Core/Postgres.
public static class HybridMatchingCalculator
{
    // A job with no required skills has nothing to mismatch on, so it counts as a full match
    // on this axis rather than zero (which would unfairly penalize skill-agnostic postings).
    public static double CalculateSkillOverlap(IReadOnlySet<Guid> personSkillIds, IReadOnlySet<Guid> requiredSkillIds)
    {
        if (requiredSkillIds.Count == 0)
            return 1.0;

        var matched = requiredSkillIds.Count(personSkillIds.Contains);
        return (double)matched / requiredSkillIds.Count;
    }

    public static double CalculateScore(
        double skillOverlap,
        double semanticSimilarity,
        double structuredWeight,
        double semanticWeight) =>
        structuredWeight * skillOverlap + semanticWeight * semanticSimilarity;
}
