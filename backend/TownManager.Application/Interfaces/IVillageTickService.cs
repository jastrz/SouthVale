namespace TownManager.Application.Interfaces;

public interface IVillageTickService
{
    Task ExecuteAsync(CancellationToken ct);
}
