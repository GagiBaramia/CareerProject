using CareerProject.Shared.Events;

namespace CareerProject.Shared.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : EventBase;
}
