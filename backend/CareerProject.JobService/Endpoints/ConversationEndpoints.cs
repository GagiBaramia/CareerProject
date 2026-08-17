using System.Security.Claims;
using CareerProject.JobService.Auth;
using CareerProject.JobService.Dtos;
using CareerProject.JobService.Hubs;
using CareerProject.Shared.Data;
using CareerProject.Shared.Entities;
using CareerProject.Shared.Validation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CareerProject.JobService.Endpoints;

public static class ConversationEndpoints
{
    public static void MapConversationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/conversations").WithTags("Conversations").RequireAuthorization();

        group.MapGet("/", GetMyConversations);
        group.MapGet("/{id:guid}/messages", GetMessages);
        group.MapPost("/{id:guid}/messages", SendMessage);
    }

    private static async Task<IResult> GetMyConversations(
        ClaimsPrincipal user, CareerProjectDbContext db, CancellationToken cancellationToken)
    {
        var userId = CurrentUserResolver.GetUserId(user);

        var conversations = await db.Conversations
            .Include(c => c.Application).ThenInclude(a => a.Job).ThenInclude(j => j.Company)
            .Include(c => c.Application).ThenInclude(a => a.Person)
            .Include(c => c.Messages)
            .Where(c => c.CandidateUserId == userId || c.CompanyUserId == userId)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .ToListAsync(cancellationToken);

        var results = conversations.Select(c =>
        {
            var isCandidate = c.CandidateUserId == userId;
            var lastMessage = c.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
            var unreadCount = c.Messages.Count(m => !m.IsRead && m.SenderUserId != userId);

            return new ConversationSummaryResponse
            {
                Id = c.Id,
                ApplicationId = c.ApplicationId,
                JobId = c.Application.JobId,
                JobTitle = c.Application.Job.Title,
                OtherPartyUserId = isCandidate ? c.CompanyUserId : c.CandidateUserId,
                OtherPartyName = isCandidate ? c.Application.Job.Company.Name : c.Application.Person.FullName,
                OtherPartyImageUrl = isCandidate ? c.Application.Job.Company.LogoUrl : c.Application.Person.PhotoUrl,
                LastMessagePreview = lastMessage?.Content,
                LastMessageAt = c.LastMessageAt,
                UnreadCount = unreadCount,
            };
        });

        return Results.Ok(results);
    }

    private static async Task<IResult> GetMessages(
        Guid id, ClaimsPrincipal user, CareerProjectDbContext db, CancellationToken cancellationToken)
    {
        var userId = CurrentUserResolver.GetUserId(user);

        var conversation = await db.Conversations.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (conversation is null)
            return Results.NotFound();

        if (conversation.CandidateUserId != userId && conversation.CompanyUserId != userId)
            return Results.Forbid();

        var messages = await db.DirectMessages
            .Where(m => m.ConversationId == id)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        // Viewing the conversation marks the other party's messages as read.
        var unreadFromOtherParty = messages.Where(m => !m.IsRead && m.SenderUserId != userId).ToList();
        if (unreadFromOtherParty.Count > 0)
        {
            var now = DateTime.UtcNow;
            foreach (var message in unreadFromOtherParty)
            {
                message.IsRead = true;
                message.ReadAt = now;
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        return Results.Ok(messages.Select(ToResponse));
    }

    private static async Task<IResult> SendMessage(
        Guid id,
        SendMessageRequest request,
        ClaimsPrincipal user,
        CareerProjectDbContext db,
        IHubContext<ChatHub> hub,
        CancellationToken cancellationToken)
    {
        if (!RequestValidator.TryValidate(request, out var errors))
            return Results.ValidationProblem(errors);

        var userId = CurrentUserResolver.GetUserId(user);

        var conversation = await db.Conversations.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (conversation is null)
            return Results.NotFound();

        if (conversation.CandidateUserId != userId && conversation.CompanyUserId != userId)
            return Results.Forbid();

        var message = new DirectMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = id,
            SenderUserId = userId,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow,
            IsRead = false,
        };

        db.DirectMessages.Add(message);
        conversation.LastMessageAt = message.CreatedAt;
        await db.SaveChangesAsync(cancellationToken);

        var response = ToResponse(message);
        await hub.Clients.Group(ChatHub.GroupName(id)).SendAsync("ReceiveMessage", response, cancellationToken);

        return Results.Created($"/api/conversations/{id}/messages/{message.Id}", response);
    }

    private static DirectMessageResponse ToResponse(DirectMessage message) => new()
    {
        Id = message.Id,
        ConversationId = message.ConversationId,
        SenderUserId = message.SenderUserId,
        Content = message.Content,
        CreatedAt = message.CreatedAt,
        IsRead = message.IsRead,
    };
}
