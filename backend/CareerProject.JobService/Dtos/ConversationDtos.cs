using System.ComponentModel.DataAnnotations;

namespace CareerProject.JobService.Dtos;

public class ConversationSummaryResponse
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid JobId { get; set; }
    public string JobTitle { get; set; } = null!;
    public Guid OtherPartyUserId { get; set; }
    public string OtherPartyName { get; set; } = null!;
    public string? OtherPartyImageUrl { get; set; }
    public string? LastMessagePreview { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
}

public class DirectMessageResponse
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderUserId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
}

public class SendMessageRequest
{
    [Required, MinLength(1), MaxLength(4000)]
    public string Content { get; set; } = null!;
}
