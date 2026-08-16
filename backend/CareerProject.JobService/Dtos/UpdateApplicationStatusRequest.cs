using System.ComponentModel.DataAnnotations;

namespace CareerProject.JobService.Dtos;

public class UpdateApplicationStatusRequest
{
    [Required]
    public string Status { get; set; } = null!;
}
