using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TownManager.Application.Interfaces;
using TownManager.Application.Llm;

namespace TownManager.Infrastructure.Llm;

public class LlmApiClient(HttpClient http, LlmPlayerConfig config) : ILlmApiClient
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = false,
    };

    public async Task<LlmApiResponse> CallAsync(string prompt, CancellationToken ct = default)
    {
        object body = config.EnableThinking
            ? new
            {
                model = config.Model,
                messages = new[] { new { role = "system", content = prompt } },
                temperature = 0.7,
                max_tokens = config.ThinkingTokens,
                thinking = new { type = "enabled" },
                stream = false,
                reasoningEffort = "medium"
            }
            : new
            {
                model = config.Model,
                messages = new[] { new { role = "system", content = prompt } },
                temperature = 0.7,
                max_tokens = config.NonThinkingTokens,
                thinking = new { type = "disabled" },
                stream = false,
                reasoningEffort = "medium"
            };

        var request = new HttpRequestMessage(HttpMethod.Post, config.ApiUrl.TrimEnd('/'))
        {
            Content = JsonContent.Create(body, options: JsonOpts),
        };
        request.Headers.Authorization = new("Bearer", config.ApiKey);

        using var response = await http.SendAsync(request, ct);
        var rawBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            return LlmApiResponse.Fail((int)response.StatusCode, rawBody);

        var parsed = JsonSerializer.Deserialize<OpenAiResponse>(rawBody, JsonOpts);
        var msg = parsed?.Choices?.FirstOrDefault()?.Message;
        var content = msg?.Content ?? msg?.ReasoningContent;
        return string.IsNullOrEmpty(content)
            ? LlmApiResponse.Fail(0, rawBody)
            : LlmApiResponse.Ok(content, rawBody);
    }

    private record OpenAiResponse
    {
        public OpenAiChoice[]? Choices { get; init; }
    }

    private record OpenAiChoice
    {
        public OpenAiMessage? Message { get; init; }
    }

    private record OpenAiMessage
    {
        public string? Content { get; init; }
        public string? ReasoningContent { get; init; }
    }
}
