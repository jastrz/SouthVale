using TownManager.Application.Common;

namespace TownManager.Application.Interfaces;

public record LoginResult(string AccessToken, string RefreshToken);

public interface IAuthService
{
    Task<Result<LoginResult>> LoginAsync(string email, string password, CancellationToken ct);
    Task<Result<string>> RegisterAsync(string email, string password, string username, CancellationToken ct);
}