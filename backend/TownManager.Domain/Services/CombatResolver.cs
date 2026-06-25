using TownManager.Domain.Config;
using TownManager.Domain.Entities;
using TownManager.Domain.Enums;

namespace TownManager.Domain.Services;

public class CombatResolver
{
    private const double K = 1.5;
    
    public static CombatResult Resolve(Troops attackers, Troops defenders, Resources defenderResources)
    {
        CombatResult result = new(Troops.Zero, Troops.Zero, Resources.Zero);
        
        double attackPower = GetAttackPower(attackers);
        double defensePower = GetDefensePower(defenders);
        
        var attackerLossRatio = Math.Pow(defensePower / attackPower, K);
        var defenderLossRatio = Math.Pow(attackPower / defensePower, K);

        var survivingAttackers = ApplyLosses(attackers, attackerLossRatio);
        var survivingDefenders = ApplyLosses(defenders, defenderLossRatio);

        int attackerCapacity = survivingAttackers.Swordsmen * TroopsConfig.Get(TroopType.Swordsman).Stats.CarryCapacity 
                               + survivingAttackers.Archers * TroopsConfig.Get(TroopType.Archer).Stats.CarryCapacity;

        Resources attackerLoot = defenderResources.Multiply(attackerCapacity * (1d / Enum.GetValues<ResourceType>().Length));

        return new CombatResult(survivingAttackers, survivingDefenders, attackerLoot);
    }
    
    private static double GetAttackPower(Troops troops) =>
        TroopsConfig.Get(TroopType.Swordsman).Stats.Attack * troops.Swordsmen +
        TroopsConfig.Get(TroopType.Archer).Stats.Attack * troops.Archers;
    
    private static double GetDefensePower(Troops troops) =>
        TroopsConfig.Get(TroopType.Swordsman).Stats.Defense * troops.Swordsmen +
        TroopsConfig.Get(TroopType.Archer).Stats.Defense * troops.Archers;

    private static Troops ApplyLosses(Troops troops, double lossRatio)
    {
        double ratio = Math.Clamp(lossRatio, 0, 1);
        
        return new Troops(
            (int)(troops.Swordsmen * (1 - ratio)), 
            (int)(troops.Archers * (1 - ratio))
        );
    }
}

public record CombatResult(Troops AttackerTroops, Troops DefenderTroops, Resources AttackerLoot);