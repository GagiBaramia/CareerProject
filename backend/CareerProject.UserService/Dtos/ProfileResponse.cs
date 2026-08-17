namespace CareerProject.UserService.Dtos;

public class ProfileResponse
{
    public string FullName { get; set; } = null!;
    public string? Headline { get; set; }
    public string? CvSummary { get; set; }
    public string? Location { get; set; }
    public string? PhotoUrl { get; set; }
    public List<ProfileSkillDto> Skills { get; set; } = [];
}
