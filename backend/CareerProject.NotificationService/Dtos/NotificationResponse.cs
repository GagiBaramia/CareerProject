namespace CareerProject.NotificationService.Dtos;

public class NotificationResponse
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!;
    public string Message { get; set; } = null!;
    public Guid? RelatedEntityId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
