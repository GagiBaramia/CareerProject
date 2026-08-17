using System.ComponentModel.DataAnnotations;

namespace CareerProject.RecommendationService.Dtos;

public class AiChatRequest
{
    [Required, MinLength(1)]
    public string Message { get; set; } = null!;
}
