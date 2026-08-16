using CareerProject.RecommendationService.Services;
using CareerProject.Shared.Events;
using CareerProject.Shared.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CareerProject.RecommendationService.Consumers;

public class ProfileCreatedConsumer(IServiceScopeFactory scopeFactory, ILogger<ProfileCreatedConsumer> logger)
    : RabbitMqConsumerBase<ProfileCreated>(logger)
{
    protected override string QueueName => "recommendation.profile.created";
    protected override string RoutingKey => "profile.created";

    protected override async Task HandleAsync(ProfileCreated @event, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<PersonProfileEmbeddingService>();
        await service.RecomputeAsync(@event.EntityId, cancellationToken);
    }
}
