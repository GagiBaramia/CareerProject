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
    public string WorkFormat { get; set; } = null!;

    [Required]
    public string Location { get; set; } = null!;

    public int? SalaryMin { get; set; }
    public int? SalaryMax { get; set; }
    public string? SalaryCurrency { get; set; }

    public List<JobSkillInput> RequiredSkills { get; set; } = [];
}

public class JobSkillInput
{
    [Required]
    public Guid SkillId { get; set; }

    [Required]
    public string RequiredLevel { get; set; } = null!;
}
