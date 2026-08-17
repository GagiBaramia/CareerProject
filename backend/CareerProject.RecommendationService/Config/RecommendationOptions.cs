namespace CareerProject.RecommendationService.Config;

public class RecommendationOptions
{
    public const string SectionName = "Recommendation";

    public double StructuredWeight { get; set; }
    public double SemanticWeight { get; set; }
    public int CacheTtlSeconds { get; set; }
}
