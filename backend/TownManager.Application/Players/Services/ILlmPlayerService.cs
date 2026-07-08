namespace TownManager.Application.Players.Services;

public interface ILlmPlayerService
{
    Task ExecuteAsync(CancellationToken ct);
}
