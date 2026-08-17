namespace CareerProject.Shared.Entities;

// Auto-created the moment a Company accepts a candidate's Application (see
// ApplicationEndpoints.UpdateApplicationStatus in CareerProject.JobService) - one Application
// can never have more than one Conversation (enforced by a unique index on ApplicationId).
public class Conversation
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid CandidateUserId { get; set; }
    public Guid CompanyUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastMessageAt { get; set; }

    public Application Application { get; set; } = null!;
    public ICollection<DirectMessage> Messages { get; set; } = new List<DirectMessage>();
}
