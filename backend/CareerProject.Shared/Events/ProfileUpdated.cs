namespace CareerProject.Shared.Events;

public sealed record ProfileUpdated : EventBase
{
    public override string RoutingKey => "profile.updated";
}
