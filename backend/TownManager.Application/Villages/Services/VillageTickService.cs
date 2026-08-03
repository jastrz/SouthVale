using Microsoft.Extensions.Logging;
using TownManager.Application.Interfaces;
using TownManager.Domain.Config;

namespace TownManager.Application.Villages.Services;

/// Materializes production and upkeep for every village.
public class VillageTickService(
    IVillageRepository villageRepo,
    ILogger<VillageTickService> logger) : IVillageTickService
{
    public async Task ExecuteAsync(CancellationToken ct)
    {
        var villages = await villageRepo.GetAllAsync(ct);
        foreach (var v in villages)
        {
            var effects = BuildingConfig.AggregateEffects(v.Buildings);
            v.Tick(effects);
            await villageRepo.SaveChangesReloadOnConflictAsync(v, ct);
        }

        logger.LogInformation("VillageTickService: Tick!");
    }
}
