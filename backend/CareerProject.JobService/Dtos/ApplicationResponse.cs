namespace CareerProject.JobService.Dtos;

public class ApplicationResponse
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string JobTitle { get; set; } = null!;
    public Guid PersonId { get; set; }
    public string PersonName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime AppliedAt { get; set; }
}
