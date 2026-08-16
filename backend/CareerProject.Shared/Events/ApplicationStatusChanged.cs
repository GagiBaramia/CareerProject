namespace CareerProject.Shared.Events;

public sealed record ApplicationStatusChanged : EventBase
{
    public override string RoutingKey => "application.status_changed";
}
