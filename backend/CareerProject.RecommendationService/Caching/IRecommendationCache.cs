using CareerProject.RecommendationService.Dtos;

namespace CareerProject.RecommendationService.Caching;

public interface IRecommendationCache
{
    Task<List<JobRecommendationResponse>?> GetAsync(Guid personProfileId, CancellationToken cancellationToken);

    Task SetAsync(Guid personProfileId, List<JobRecommendationResponse> jobs, CancellationToken cancellationToken);

    Task InvalidateAsync(Guid personProfileId, CancellationToken cancellationToken);

    Task InvalidateAllAsync(CancellationToken cancellationToken);
}
