using System.Security.Claims;
using CareerProject.RecommendationService.Dtos;
using CareerProject.RecommendationService.Services;
using CareerProject.Shared.Data;
using CareerProject.Shared.Validation;
using Microsoft.EntityFrameworkCore;

namespace CareerProject.RecommendationService.Endpoints;

public static class AiChatEndpoints
{
    public static void MapAiChatEndpoints(this WebApplication app)
    {
        app.MapGroup("/api/ai")
            .WithTags("AI Chat")
            .RequireAuthorization("PersonOnly")
            .MapPost("/chat", Chat);
    }

    private static async Task<IResult> Chat(
        AiChatRequest request,
        ClaimsPrincipal user,
        CareerProjectDbContext db,
        AiChatService chatService,
        CancellationToken cancellationToken)
    {
        if (!RequestValidator.TryValidate(request, out var errors))
            return Results.ValidationProblem(errors);

        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub")!);

        var profile = await db.PersonProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        if (profile is null)
            return Results.NotFound(new { message = "Person profile not found." });

        var response = await chatService.AskAsync(profile.Id, request.Message, cancellationToken);
        return Results.Ok(response);
    }
}
