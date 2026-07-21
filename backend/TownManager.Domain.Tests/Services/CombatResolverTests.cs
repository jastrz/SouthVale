using FluentAssertions;
using TownManager.Domain.Entities;
using TownManager.Domain.Enums;
using TownManager.Domain.Services;

namespace TownManager.Domain.Tests.Services;

public class CombatResolverTests
{
    [Fact]
    public void NoAttackers_DefendersUntouched_NoLoot()
    {
        var result = CombatResolver.Resolve(
            Troops.Zero,
            new Troops(swordsmen: 100),
            new Resources(500, 400, 300, 200)
        );

        result.AttackerTroops.Should().BeEquivalentTo(Troops.Zero);
        result.DefenderTroops.Should().BeEquivalentTo(new Troops(swordsmen: 100));
        result.AttackerLoot.IsEmpty().Should().BeTrue();
    }
    
    [Fact]
    public void NoDefenders_AttackersUnaffected_FullLoot()
    {
        var resources = new Resources(wood: 100, clay: 200, iron: 300, beer: 400);

        var result = CombatResolver.Resolve(
            new Troops(swordsmen: 50),
            Troops.Zero,
            resources
        );

        // No defenders - no defense power - defenders take no losses
        result.AttackerTroops.Should().BeEquivalentTo(new Troops(swordsmen: 50));
        result.DefenderTroops.Should().BeEquivalentTo(Troops.Zero);
        
        result.AttackerLoot.Wood.Should().Be(resources.Wood);
        result.AttackerLoot.Clay.Should().Be(resources.Clay);
        result.AttackerLoot.Iron.Should().Be(resources.Iron);
        result.AttackerLoot.Beer.Should().Be(resources.Beer);
    }

    [Fact]
    public void MixedTroops_CombinesSwordsmenAndArchers()
    {
        // Overwhelming force ratio ensures clean wipe (avoid floating-point edge cases)
        var result = CombatResolver.Resolve(
            new Troops(swordsmen: 100, archers: 100),
            new Troops(swordsmen: 10, archers: 10),
            Resources.Zero
        );
        
        result.DefenderTroops.Should().BeEquivalentTo(Troops.Zero);
        result.AttackerTroops.Get(TroopType.Swordsman).Should().BeGreaterThan(0);
        result.AttackerTroops.Get(TroopType.Archer).Should().BeGreaterThan(0);
    }

    [Fact]
    public void Settlers_DoNotContributeToCombat()
    {
        var result = CombatResolver.Resolve(
            new Troops(swordsmen: 100, settlers: 100),
            new Troops(swordsmen: 100),
            Resources.Zero
        );

        // Settlers have 0 attack/defense — survive combat untouched
        result.AttackerTroops.Get(TroopType.Swordsman).Should().BeGreaterThan(0);
        result.AttackerTroops.Get(TroopType.Settler).Should().Be(100);
    }

    [Fact]
    public void Cranny_ReducesLoot()
    {
        var resources = new Resources(wood: 1000, clay: 1000, iron: 1000, beer: 1000);
        var crannyCap = 800;

        var lootable = new Resources(
            Math.Max(0, resources.Wood - crannyCap),
            Math.Max(0, resources.Clay - crannyCap),
            Math.Max(0, resources.Iron - crannyCap),
            Math.Max(0, resources.Beer - crannyCap));

        var withoutCranny = CombatResolver.Resolve(
            new Troops(swordsmen: 100), Troops.Zero, resources);
        var withCranny = CombatResolver.Resolve(
            new Troops(swordsmen: 100), Troops.Zero, lootable);

        withCranny.AttackerLoot.Wood.Should().BeLessThan(withoutCranny.AttackerLoot.Wood);
        withCranny.AttackerLoot.Clay.Should().BeLessThan(withoutCranny.AttackerLoot.Clay);
        withCranny.AttackerLoot.Iron.Should().BeLessThan(withoutCranny.AttackerLoot.Iron);
        withCranny.AttackerLoot.Beer.Should().BeLessThan(withoutCranny.AttackerLoot.Beer);
    }

    [Fact]
    public void WallMultiplier_ReducesDefenderLosses()
    {
        var attackers = new Troops(swordsmen: 10);
        var defenders = new Troops(swordsmen: 50);

        var withoutWall = CombatResolver.Resolve(attackers, defenders, Resources.Zero, defenseMultiplier: 1.0);
        var withWall = CombatResolver.Resolve(attackers, defenders, Resources.Zero, defenseMultiplier: 2.0);

        // 2x defense power → fewer defender casualties
        withWall.DefenderTroops.TotalCount.Should().BeGreaterThan(withoutWall.DefenderTroops.TotalCount);
    }

    [Fact]
    public void TroopCounts_NeverNegative()
    {
        var result = CombatResolver.Resolve(
            new Troops(swordsmen: 1),
            new Troops(swordsmen: 1000),
            Resources.Zero
        );

        result.AttackerTroops.Get(TroopType.Swordsman).Should().BeGreaterThanOrEqualTo(0);
        result.AttackerTroops.Get(TroopType.Archer).Should().BeGreaterThanOrEqualTo(0);
        result.AttackerTroops.Get(TroopType.Settler).Should().BeGreaterThanOrEqualTo(0);
        result.DefenderTroops.Get(TroopType.Swordsman).Should().BeGreaterThanOrEqualTo(0);
        result.DefenderTroops.Get(TroopType.Archer).Should().BeGreaterThanOrEqualTo(0);
        result.DefenderTroops.Get(TroopType.Settler).Should().BeGreaterThanOrEqualTo(0);
    }
}
