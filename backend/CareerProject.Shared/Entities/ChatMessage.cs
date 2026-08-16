namespace CareerProject.Shared.Entities;

public class ChatMessage
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public string Role { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public PersonProfile Person { get; set; } = null!;
}
