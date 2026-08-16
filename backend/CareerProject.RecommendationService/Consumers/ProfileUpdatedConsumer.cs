using CareerProject.RecommendationService.Services;
using CareerProject.Shared.Events;
using CareerProject.Shared.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CareerProject.RecommendationService.Consumers;

public class ProfileUpdatedConsumer(IServiceScopeFactory scopeFactory, ILogger<ProfileUpdatedConsumer> logger)
    : RabbitMqConsumerBase<ProfileUpdated>(logger)
{
    protected override string QueueName => "recommendation.profile.updated";
    protected override string RoutingKey => "profile.updated";

    protected override async Task HandleAsync(ProfileUpdated @event, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<PersonProfileEmbeddingService>();
        await service.RecomputeAsync(@event.EntityId, cancellationToken);
    }
}
