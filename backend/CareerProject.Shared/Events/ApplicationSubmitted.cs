namespace CareerProject.Shared.Events;

public sealed record ApplicationSubmitted : EventBase
{
    public override string RoutingKey => "application.submitted";
}
