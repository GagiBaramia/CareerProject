namespace CareerProject.Shared.Events;

public sealed record ProfileCreated : EventBase
{
    public override string RoutingKey => "profile.created";
}
