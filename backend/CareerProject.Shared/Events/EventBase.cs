namespace CareerProject.Shared.Events;

public abstract record EventBase
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public required Guid EntityId { get; init; }

    public abstract string RoutingKey { get; }
}
