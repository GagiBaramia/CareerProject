using System.ComponentModel.DataAnnotations;

namespace CareerProject.UserService.Dtos;

public class UpdateProfileRequest
{
    [Required, MinLength(1)]
    public string FullName { get; set; } = null!;

    public string? Headline { get; set; }
    public string? CvSummary { get; set; }
    public string? Location { get; set; }
}
