using Pgvector;

namespace CareerProject.Shared.Entities;

public class PersonProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string? Headline { get; set; }
    public string? CvSummary { get; set; }
    public string? Location { get; set; }
    public string? PhotoUrl { get; set; }
    public Vector? Embedding { get; set; }

    public User User { get; set; } = null!;
    public ICollection<PersonSkill> PersonSkills { get; set; } = new List<PersonSkill>();
    public ICollection<Application> Applications { get; set; } = new List<Application>();
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
}
