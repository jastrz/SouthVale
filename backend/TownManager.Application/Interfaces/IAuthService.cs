using TownManager.Application.Common;

namespace TownManager.Application.Interfaces;

public record LoginResult(string AccessToken, string RefreshToken, string Username)
{
    public string? Password { get; init; }
}

public interface IAuthService
{
    Task<Result<LoginResult>> LoginAsync(string email, string password, CancellationToken ct);
    Task<Result<LoginResult>> RegisterAsync(string email, string password, string username, CancellationToken ct);
    Task<Result<LoginResult>> RegisterGuestAsync(CancellationToken ct);
    Task<Result> ClaimGuestAsync(string userId, string email, string newPassword, string username, CancellationToken ct);
    Task<Result<bool>> DeleteAsync(string userId, string password, CancellationToken ct);
}