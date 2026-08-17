using CareerProject.NotificationService.Services;
using CareerProject.Shared.Events;
using CareerProject.Shared.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CareerProject.NotificationService.Consumers;

public class ApplicationStatusChangedConsumer(IServiceScopeFactory scopeFactory, ILogger<ApplicationStatusChangedConsumer> logger)
    : RabbitMqConsumerBase<ApplicationStatusChanged>(logger)
{
    protected override string QueueName => "notification.application.status_changed";
    protected override string RoutingKey => "application.status_changed";

    protected override async Task HandleAsync(ApplicationStatusChanged @event, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ApplicationNotificationService>();
        await service.NotifyApplicationStatusChangedAsync(@event.EntityId, cancellationToken);
    }
}
