using CareerProject.RecommendationService.Config;
using CareerProject.RecommendationService.Dtos;
using CareerProject.Shared.Data;
using CareerProject.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace CareerProject.RecommendationService.Services;

public class AiChatService
{
    private const string SystemPromptTemplate = """
        შენ ხარ CareerProject-ის AI Job Assistant - ვაკანსიების ძიებაში დამხმარე ასისტენტი.
        პასუხი დააფუძნე მხოლოდ ქვემოთ მოცემულ რეალურ ვაკანსიებზე. კატეგორიულად აკრძალულია ისეთი ვაკანსიის,
        კომპანიის ან დეტალის გამოგონება, რომელიც ამ სიაში არ არის.
        თუ სიაში მომხმარებლის მოთხოვნის შესაბამისი ვაკანსია არ არის, პირდაპირ და პატიოსნად უთხარი, რომ
        ამჟამად შესაბამისი ვაკანსია ვერ მოიძებნა - არ გამოიგონო ალტერნატივა.
        უპასუხე ქართულად, მოკლედ და კონკრეტულად.

        ხელმისაწვდომი ვაკანსიები:
        {0}
        """;

    private readonly CareerProjectDbContext _db;
    private readonly GeminiEmbeddingClient _embeddingClient;
    private readonly GeminiChatClient _chatClient;
    private readonly AiChatOptions _options;

    public AiChatService(
        CareerProjectDbContext db,
        GeminiEmbeddingClient embeddingClient,
        GeminiChatClient chatClient,
        IOptions<AiChatOptions> options)
    {
        _db = db;
        _embeddingClient = embeddingClient;
        _chatClient = chatClient;
        _options = options.Value;
    }

    public async Task<AiChatResponse> AskAsync(Guid personId, string message, CancellationToken cancellationToken = default)
    {
        _db.ChatMessages.Add(new ChatMessage
        {
            Id = Guid.NewGuid(),
            PersonId = personId,
            Role = "user",
            Content = message,
            CreatedAt = DateTime.UtcNow,
        });

        var queryEmbedding = await _embeddingClient.EmbedAsync(message, "RETRIEVAL_QUERY", cancellationToken);
        var queryVector = new Vector(queryEmbedding);

        var relevantJobs = await _db.Jobs
            .Include(j => j.Company)
            .Where(j => j.Embedding != null)
            .OrderBy(j => j.Embedding!.CosineDistance(queryVector))
            .Take(_options.TopKJobs)
            .ToListAsync(cancellationToken);

        var systemPrompt = string.Format(SystemPromptTemplate, BuildContext(relevantJobs));
        var reply = await _chatClient.GenerateAsync(systemPrompt, message, cancellationToken);

        _db.ChatMessages.Add(new ChatMessage
        {
            Id = Guid.NewGuid(),
            PersonId = personId,
            Role = "assistant",
            Content = reply,
            CreatedAt = DateTime.UtcNow,
        });

        await _db.SaveChangesAsync(cancellationToken);

        return new AiChatResponse
        {
            Reply = reply,
            JobIds = relevantJobs.Select(j => j.Id).ToList(),
            ReferencedJobs = relevantJobs.Select(j => new AiChatJobReferenceDto
            {
                JobId = j.Id,
                Title = j.Title,
                CompanyName = j.Company.Name,
            }).ToList(),
        };
    }

    private static string BuildContext(List<Job> jobs)
    {
        if (jobs.Count == 0)
            return "(ამჟამად ვერცერთი ვაკანსია ვერ მოიძებნა)";

        return string.Join("\n\n", jobs.Select(j =>
            $"- ID: {j.Id}\n  სათაური: {j.Title}\n  კომპანია: {j.Company.Name}\n  ლოკაცია: {j.Location}\n" +
            $"  დასაქმების ტიპი: {j.EmploymentType}\n  ფორმატი: {j.WorkFormat}\n  აღწერა: {j.Description}"));
    }
}
