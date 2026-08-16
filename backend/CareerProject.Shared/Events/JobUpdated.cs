namespace CareerProject.Shared.Events;

public sealed record JobUpdated : EventBase
{
    public override string RoutingKey => "job.updated";
}
