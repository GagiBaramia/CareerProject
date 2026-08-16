using Pgvector;

namespace CareerProject.Shared.Entities;

public class Job
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string EmploymentType { get; set; } = null!;
    public string Location { get; set; } = null!;
    public Vector? Embedding { get; set; }
    public DateTime CreatedAt { get; set; }

    public Company Company { get; set; } = null!;
    public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
    public ICollection<Application> Applications { get; set; } = new List<Application>();
}
