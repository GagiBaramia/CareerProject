using Microsoft.Extensions.DependencyInjection;

namespace CareerProject.Shared.Messaging;

public static class EventPublisherServiceCollectionExtensions
{
    public static IServiceCollection AddCareerProjectEventPublisher(this IServiceCollection services) =>
        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
}
