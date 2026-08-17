using CareerProject.JobService.Auth;
using CareerProject.Shared.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CareerProject.JobService.Hubs;

// Realtime delivery only - message persistence and history stay on the REST endpoints
// (ConversationEndpoints). A connection joins one SignalR group per conversation it's a
// participant of; ConversationEndpoints.SendMessage broadcasts to that group after saving.
[Authorize]
public class ChatHub(CareerProjectDbContext db) : Hub
{
    public static string GroupName(Guid conversationId) => $"conversation-{conversationId}";

    public async Task JoinConversation(Guid conversationId)
    {
        var userId = CurrentUserResolver.GetUserId(Context.User!);

        var conversation = await db.Conversations.FirstOrDefaultAsync(c => c.Id == conversationId);
        if (conversation is null || (conversation.CandidateUserId != userId && conversation.CompanyUserId != userId))
        {
            throw new HubException("Not a participant of this conversation.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(conversationId));
    }
}
