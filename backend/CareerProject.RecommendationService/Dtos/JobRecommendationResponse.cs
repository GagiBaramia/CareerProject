namespace CareerProject.RecommendationService.Dtos;

public class JobRecommendationResponse
{
    public Guid JobId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string EmploymentType { get; set; } = null!;
    public string WorkFormat { get; set; } = null!;
    public string Location { get; set; } = null!;
    public int? SalaryMin { get; set; }
    public int? SalaryMax { get; set; }
    public string? SalaryCurrency { get; set; }

    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = null!;

    // 0..1 - the frontend renders this as a percentage.
    public double Score { get; set; }
    public double SkillOverlap { get; set; }
    public double SemanticSimilarity { get; set; }

    public List<RecommendationSkillDto> RequiredSkills { get; set; } = [];
}

public class RecommendationSkillDto
{
    public Guid SkillId { get; set; }
    public string SkillName { get; set; } = null!;
    public string RequiredLevel { get; set; } = null!;
}
