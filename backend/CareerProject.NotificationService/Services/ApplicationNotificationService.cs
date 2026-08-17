using CareerProject.Shared.Data;
using CareerProject.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CareerProject.NotificationService.Services;

public class ApplicationNotificationService(
    CareerProjectDbContext db,
    ILogger<ApplicationNotificationService> logger)
{
    public async Task NotifyApplicationSubmittedAsync(Guid applicationId, CancellationToken cancellationToken)
    {
        var application = await db.Applications
            .Include(a => a.Job).ThenInclude(j => j.Company)
            .Include(a => a.Person)
            .FirstOrDefaultAsync(a => a.Id == applicationId, cancellationToken);

        if (application is null)
        {
            logger.LogWarning("Application {ApplicationId} not found, skipping notification.", applicationId);
            return;
        }

        await CreateNotification(
            recipientUserId: application.Job.Company.UserId,
            type: "ApplicationSubmitted",
            message: $"{application.Person.FullName}-მა გამოგიგზავნათ განაცხადი \"{application.Job.Title}\" ვაკანსიაზე.",
            relatedEntityId: application.Id,
            cancellationToken);
    }

    public async Task NotifyApplicationStatusChangedAsync(Guid applicationId, CancellationToken cancellationToken)
    {
        var application = await db.Applications
            .Include(a => a.Job)
            .Include(a => a.Person)
            .FirstOrDefaultAsync(a => a.Id == applicationId, cancellationToken);

        if (application is null)
        {
            logger.LogWarning("Application {ApplicationId} not found, skipping notification.", applicationId);
            return;
        }

        await CreateNotification(
            recipientUserId: application.Person.UserId,
            type: "ApplicationStatusChanged",
            message: $"თქვენი განაცხადის სტატუსი \"{application.Job.Title}\" ვაკანსიაზე შეიცვალა: {StatusLabel(application.Status)}.",
            relatedEntityId: application.Id,
            cancellationToken);
    }

    private async Task CreateNotification(
        Guid recipientUserId, string type, string message, Guid relatedEntityId, CancellationToken cancellationToken)
    {
        db.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            RecipientUserId = recipientUserId,
            Type = type,
            Message = message,
            RelatedEntityId = relatedEntityId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
        });

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created {Type} notification for user {UserId}.", type, recipientUserId);
    }

    private static string StatusLabel(ApplicationStatus status) => status switch
    {
        ApplicationStatus.Submitted => "გაგზავნილია",
        ApplicationStatus.InReview => "განხილვაშია",
        ApplicationStatus.Interview => "მოწვეულია გასაუბრებაზე",
        ApplicationStatus.Rejected => "უარყოფილია",
        ApplicationStatus.Accepted => "მიღებულია",
        _ => status.ToString(),
    };
}
