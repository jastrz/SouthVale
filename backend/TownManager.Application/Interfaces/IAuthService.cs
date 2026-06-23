using TownManager.Application.Common;

namespace TownManager.Application.Interfaces;

public record LoginResult(string AccessToken, string RefreshToken, string Username);

public interface IAuthService
{
    Task<Result<LoginResult>> LoginAsync(string email, string password, CancellationToken ct);
    Task<Result<LoginResult>> RegisterAsync(string email, string password, string username, CancellationToken ct);
}