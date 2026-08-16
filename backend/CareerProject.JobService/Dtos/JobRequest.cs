using System.ComponentModel.DataAnnotations;

namespace CareerProject.JobService.Dtos;

public class JobRequest
{
    [Required, MinLength(1)]
    public string Title { get; set; } = null!;

    [Required, MinLength(1)]
    public string Description { get; set; } = null!;

    [Required]
    public string EmploymentType { get; set; } = null!;

    [Required]
    public string Location { get; set; } = null!;

    public List<JobSkillInput> RequiredSkills { get; set; } = [];
}

public class JobSkillInput
{
    [Required]
    public Guid SkillId { get; set; }

    [Required]
    public string RequiredLevel { get; set; } = null!;
}
