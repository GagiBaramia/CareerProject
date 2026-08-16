namespace CareerProject.JobService.Dtos;

public class JobResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string EmploymentType { get; set; } = null!;
    public string Location { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = null!;
    public List<JobSkillDto> RequiredSkills { get; set; } = [];
}

public class JobSkillDto
{
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = null!;
    public string RequiredLevel { get; set; } = null!;
}
