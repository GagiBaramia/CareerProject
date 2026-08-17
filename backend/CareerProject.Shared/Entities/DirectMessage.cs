namespace CareerProject.Shared.Entities;

// Employer <-> candidate chat, distinct from ChatMessage (the AI assistant's own history) -
// deliberately a separate model since the two have nothing to do with each other.
public class DirectMessage
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderUserId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

    public Conversation Conversation { get; set; } = null!;
}
