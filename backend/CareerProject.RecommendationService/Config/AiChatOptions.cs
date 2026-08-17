namespace CareerProject.RecommendationService.Config;

public class AiChatOptions
{
    public const string SectionName = "AiChat";

    public int TopKJobs { get; set; }
    public AiChatRateLimitOptions RateLimit { get; set; } = new();
}

public class AiChatRateLimitOptions
{
    public int MaxRequests { get; set; }
    public int WindowSeconds { get; set; }
}
