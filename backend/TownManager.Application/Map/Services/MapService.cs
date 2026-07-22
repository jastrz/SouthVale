using TownManager.Application.Interfaces;
using TownManager.Domain.Config;
using TownManager.Domain.Entities;

namespace TownManager.Application.Map.Services;

public class MapService(IVillageRepository villageRepo) : IMapService
{
    public async Task<List<Coordinates>> GetFreeTilesAsync(int count, CancellationToken ct = default)
    {
        var occupied = new HashSet<Coordinates>(
            await villageRepo.GetAllCoordinatesAsync(ct));

        var all = (
            from x in Enumerable.Range(0, MapConfig.MapSize)
            from y in Enumerable.Range(0, MapConfig.MapSize)
            select new Coordinates(x, y)
        ).Where(c => !occupied.Contains(c) && MapTerrain.IsWalkable(c))
         .ToList();

        return all.Count <= count
            ? all
            : all.OrderBy(_ => Random.Shared.Next()).Take(count).ToList();
    }

    public bool IsWalkable(Coordinates coords) => MapTerrain.IsWalkable(coords);
    public string GetTerrainJson() => MapTerrain.GetTerrainJson();
}
