namespace TownManager.Application.Interfaces;

public interface ILlmApiClient
{
    Task<LlmApiResponse> CallAsync(string prompt, CancellationToken ct = default);
}

public record LlmApiResponse(bool Success, string? Content, int? StatusCode, string? RawBody)
{
    public static LlmApiResponse Ok(string content, string rawBody) => new(true, content, null, rawBody);
    public static LlmApiResponse Fail(int status, string rawBody) => new(false, null, status, rawBody);
}
