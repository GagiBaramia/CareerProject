using CareerProject.RecommendationService.Services;
using CareerProject.Shared.Events;
using CareerProject.Shared.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CareerProject.RecommendationService.Consumers;

public class JobCreatedConsumer(IServiceScopeFactory scopeFactory, ILogger<JobCreatedConsumer> logger)
    : RabbitMqConsumerBase<JobCreated>(logger)
{
    protected override string QueueName => "recommendation.job.created";
    protected override string RoutingKey => "job.created";

    protected override async Task HandleAsync(JobCreated @event, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<JobEmbeddingService>();
        await service.RecomputeAsync(@event.EntityId, cancellationToken);
    }
}
