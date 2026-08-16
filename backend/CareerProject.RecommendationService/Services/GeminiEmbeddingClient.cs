using System.Net.Http.Json;
using System.Text.Json;
using CareerProject.Shared.Entities;

namespace CareerProject.RecommendationService.Services;

public class GeminiEmbeddingClient
{
    private const string Model = "gemini-embedding-001";
    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeminiEmbeddingClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
            ?? throw new InvalidOperationException("GEMINI_API_KEY environment variable is not set.");
    }

    // taskType: "RETRIEVAL_DOCUMENT" for stored profile/job text, "RETRIEVAL_QUERY" for a search query (Stage 19).
    public async Task<float[]> EmbedAsync(string text, string taskType, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/{Model}:embedContent");
        request.Headers.Add("x-goog-api-key", _apiKey);
        request.Content = JsonContent.Create(
            new EmbedRequest(new EmbedContent([new EmbedPart(text)]), EmbeddingDefaults.Dimensions, taskType),
            options: JsonOptions);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<EmbedResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Gemini embedContent returned an empty response.");

        return result.Embedding.Values;
    }

    private record EmbedRequest(EmbedContent Content, int OutputDimensionality, string TaskType);
    private record EmbedContent(EmbedPart[] Parts);
    private record EmbedPart(string Text);
    private record EmbedResponse(EmbedValues Embedding);
    private record EmbedValues(float[] Values);
}
