using TownManager.Domain.Config;
using TownManager.Domain.Entities;
using TownManager.Domain.Enums;

namespace TownManager.Domain.Services;

public class CombatResolver
{
    private const double K = 1.5;

    public static CombatResult Resolve(Troops attackers, Troops defenders, Resources defenderResources)
    {
        double attackPower = GetAttackPower(attackers);
        double defensePower = GetDefensePower(defenders);

        var attackerLossRatio = Math.Pow(defensePower / attackPower, K);
        var defenderLossRatio = Math.Pow(attackPower / defensePower, K);

        var survivingAttackers = ApplyLosses(attackers, attackerLossRatio);
        var survivingDefenders = ApplyLosses(defenders, defenderLossRatio);

        int attackerCapacity = 0;
        foreach (TroopType t in Enum.GetValues<TroopType>())
        {
            var cnt = survivingAttackers.Get(t);
            if (cnt > 0) attackerCapacity += TroopsConfig.Get(t).Stats.CarryCapacity * cnt;
        }

        double totalResources = defenderResources.Wood + defenderResources.Clay + defenderResources.Iron + defenderResources.Beer;
        double lootRatio = totalResources > 0 ? Math.Min(1.0, attackerCapacity / totalResources) : 0;
        Resources attackerLoot = defenderResources.Multiply(lootRatio);

        return new CombatResult(survivingAttackers, survivingDefenders, attackerLoot);
    }

    private static double SumStat(Troops troops, Func<TroopConfig, int> stat) =>
        Enum.GetValues<TroopType>().Sum(t => troops.Get(t) * stat(TroopsConfig.Get(t)));

    private static double GetAttackPower(Troops troops) => SumStat(troops, c => c.Stats.Attack);
    private static double GetDefensePower(Troops troops) => SumStat(troops, c => c.Stats.Defense);

    private static Troops ApplyLosses(Troops troops, double lossRatio)
    {
        double ratio = Math.Clamp(lossRatio, 0, 1);
        var result = Troops.Zero;
        foreach (TroopType t in Enum.GetValues<TroopType>())
        {
            var cnt = troops.Get(t);
            if (cnt <= 0) continue;
            var cfg = TroopsConfig.Get(t);
            if (cfg.Stats.Attack == 0 && cfg.Stats.Defense == 0)
                result = result.Add(t, cnt);
            else
            {
                var remaining = (int)(cnt * (1 - ratio));
                if (remaining > 0) result = result.Add(t, remaining);
            }
        }
        return result;
    }
}

public record CombatResult(Troops AttackerTroops, Troops DefenderTroops, Resources AttackerLoot);
