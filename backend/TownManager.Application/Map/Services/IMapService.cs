using TownManager.Domain.Entities;

namespace TownManager.Application.Map.Services;

public interface IMapService
{
    Task<List<Coordinates>> GetFreeTilesAsync(int count, CancellationToken ct = default);
    bool IsWalkable(Coordinates coords);
    string GetTerrainJson();
}
