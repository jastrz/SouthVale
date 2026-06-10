using TownManager.Application.Common;

namespace TownManager.Application.Interfaces;

public interface IAuthService
{
    Task<Result> RegisterAsync(string email, string password, string username, CancellationToken ct);
    Task<Result<string>> LoginAsync(string email, string password, CancellationToken ct);
}
