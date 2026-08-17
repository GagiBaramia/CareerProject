using CareerProject.NotificationService.Services;
using CareerProject.Shared.Events;
using CareerProject.Shared.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CareerProject.NotificationService.Consumers;

public class ApplicationSubmittedConsumer(IServiceScopeFactory scopeFactory, ILogger<ApplicationSubmittedConsumer> logger)
    : RabbitMqConsumerBase<ApplicationSubmitted>(logger)
{
    protected override string QueueName => "notification.application.submitted";
    protected override string RoutingKey => "application.submitted";

    protected override async Task HandleAsync(ApplicationSubmitted @event, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ApplicationNotificationService>();
        await service.NotifyApplicationSubmittedAsync(@event.EntityId, cancellationToken);
    }
}
