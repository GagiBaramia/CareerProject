namespace CareerProject.Shared.Entities;

public class Company
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? Industry { get; set; }

    public User User { get; set; } = null!;
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
