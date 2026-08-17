namespace CareerProject.RecommendationService.Dtos;

public class AiChatResponse
{
    public string Reply { get; set; } = null!;
    public List<Guid> JobIds { get; set; } = [];
    public List<AiChatJobReferenceDto> ReferencedJobs { get; set; } = [];
}

public class AiChatJobReferenceDto
{
    public Guid JobId { get; set; }
    public string Title { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
}
