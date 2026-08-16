namespace CareerProject.JobService.Dtos;

public class CompanyProfileResponse
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Industry { get; set; }
    public string? LogoUrl { get; set; }
}
