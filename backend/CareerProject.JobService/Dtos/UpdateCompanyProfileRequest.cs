using System.ComponentModel.DataAnnotations;

namespace CareerProject.JobService.Dtos;

public class UpdateCompanyProfileRequest
{
    [Required, MinLength(1)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }
    public string? Industry { get; set; }
    public string? LogoUrl { get; set; }
}
