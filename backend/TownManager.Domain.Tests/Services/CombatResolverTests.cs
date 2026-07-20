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
        var result = CombatResolver.Resolve(
            new Troops(swordsmen: 50, archers: 50),
            new Troops(swordsmen: 50, archers: 50),
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
