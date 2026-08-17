using System.Net.Http.Json;
using System.Text.Json;

namespace CareerProject.RecommendationService.Services;

public class GeminiChatClient
{
    private const string Model = "gemini-3.6-flash";
    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeminiChatClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
            ?? throw new InvalidOperationException("GEMINI_API_KEY environment variable is not set.");
    }

    public async Task<string> GenerateAsync(string systemInstruction, string userMessage, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/{Model}:generateContent");
        request.Headers.Add("x-goog-api-key", _apiKey);
        request.Content = JsonContent.Create(
            new GenerateRequest(
                new GenerateInstruction([new GeneratePart(systemInstruction)]),
                [new GenerateContent("user", [new GeneratePart(userMessage)])]),
            options: JsonOptions);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GenerateResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Gemini generateContent returned an empty response.");

        var text = result.Candidates.FirstOrDefault()?.Content.Parts.FirstOrDefault()?.Text;
        return text ?? throw new InvalidOperationException("Gemini generateContent returned no text.");
    }

    private record GenerateRequest(GenerateInstruction SystemInstruction, GenerateContent[] Contents);
    private record GenerateInstruction(GeneratePart[] Parts);
    private record GenerateContent(string Role, GeneratePart[] Parts);
    private record GeneratePart(string Text);
    private record GenerateResponse(GenerateCandidate[] Candidates);
    private record GenerateCandidate(GenerateResponseContent Content);
    private record GenerateResponseContent(GeneratePart[] Parts);
}
