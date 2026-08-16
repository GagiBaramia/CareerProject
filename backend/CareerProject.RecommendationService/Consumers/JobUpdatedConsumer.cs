using CareerProject.RecommendationService.Services;
using CareerProject.Shared.Events;
using CareerProject.Shared.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CareerProject.RecommendationService.Consumers;

public class JobUpdatedConsumer(IServiceScopeFactory scopeFactory, ILogger<JobUpdatedConsumer> logger)
    : RabbitMqConsumerBase<JobUpdated>(logger)
{
    protected override string QueueName => "recommendation.job.updated";
    protected override string RoutingKey => "job.updated";

    protected override async Task HandleAsync(JobUpdated @event, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<JobEmbeddingService>();
        await service.RecomputeAsync(@event.EntityId, cancellationToken);
    }
}
