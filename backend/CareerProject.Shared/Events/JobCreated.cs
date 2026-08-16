namespace CareerProject.Shared.Events;

public sealed record JobCreated : EventBase
{
    public override string RoutingKey => "job.created";
}
