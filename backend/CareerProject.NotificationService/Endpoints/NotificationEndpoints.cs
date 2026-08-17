using System.Security.Claims;
using CareerProject.NotificationService.Dtos;
using CareerProject.Shared.Data;
using CareerProject.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace CareerProject.NotificationService.Endpoints;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/notifications").WithTags("Notifications").RequireAuthorization();

        group.MapGet("/", ListMyNotifications);
        group.MapPatch("/{id:guid}/read", MarkAsRead);
    }

    private static async Task<IResult> ListMyNotifications(ClaimsPrincipal user, CareerProjectDbContext db)
    {
        var userId = GetUserId(user);

        var notifications = await db.Notifications
            .Where(n => n.RecipientUserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return Results.Ok(notifications.Select(ToResponse));
    }

    private static async Task<IResult> MarkAsRead(Guid id, ClaimsPrincipal user, CareerProjectDbContext db)
    {
        var userId = GetUserId(user);

        var notification = await db.Notifications.FirstOrDefaultAsync(n => n.Id == id);
        if (notification is null)
            return Results.NotFound();

        if (notification.RecipientUserId != userId)
            return Results.Forbid();

        notification.IsRead = true;
        await db.SaveChangesAsync();

        return Results.Ok(ToResponse(notification));
    }

    private static Guid GetUserId(ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub")!);

    private static NotificationResponse ToResponse(Notification notification) => new()
    {
        Id = notification.Id,
        Type = notification.Type,
        Message = notification.Message,
        RelatedEntityId = notification.RelatedEntityId,
        IsRead = notification.IsRead,
        CreatedAt = notification.CreatedAt,
    };
}
