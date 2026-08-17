namespace CareerProject.RecommendationService.Caching;

public interface IAiChatRateLimiter
{
    // Returns false when the user has exceeded their request quota for the current window.
    Task<bool> TryAcquireAsync(Guid userId, CancellationToken cancellationToken);
}
