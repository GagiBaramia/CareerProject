namespace CareerProject.Shared.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public Guid RecipientUserId { get; set; }
    public string Type { get; set; } = null!;
    public string Message { get; set; } = null!;
    public Guid? RelatedEntityId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }

    public User RecipientUser { get; set; } = null!;
}
