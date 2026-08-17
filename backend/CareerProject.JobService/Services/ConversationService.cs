using CareerProject.Shared.Data;
using CareerProject.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace CareerProject.JobService.Services;

public class ConversationService(CareerProjectDbContext db)
{
    // Idempotent - safe to call every time an application's status is set to Accepted, even if
    // it already was. Doesn't call SaveChangesAsync itself; the caller commits this together
    // with the status change in one transaction.
    public async Task EnsureConversationForAcceptedApplicationAsync(
        Application application, Guid companyUserId, CancellationToken cancellationToken)
    {
        var alreadyExists = await db.Conversations
            .AnyAsync(c => c.ApplicationId == application.Id, cancellationToken);

        if (alreadyExists)
            return;

        db.Conversations.Add(new Conversation
        {
            Id = Guid.NewGuid(),
            ApplicationId = application.Id,
            CandidateUserId = application.Person.UserId,
            CompanyUserId = companyUserId,
            CreatedAt = DateTime.UtcNow,
        });
    }
}
