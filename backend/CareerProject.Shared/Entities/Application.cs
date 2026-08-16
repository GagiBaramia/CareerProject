namespace CareerProject.Shared.Entities;

public class Application
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public Guid PersonId { get; set; }
    public ApplicationStatus Status { get; set; }
    public DateTime AppliedAt { get; set; }

    public Job Job { get; set; } = null!;
    public PersonProfile Person { get; set; } = null!;
}
