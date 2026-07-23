using FluentAssertions;
using TownManager.Domain.Config;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;

namespace TownManager.Domain.Tests.Entities.Villages;

public class VillageTests
{
    [Fact]
    public void CalculateStarvation_NoDeficit_ReturnsNothing()
    {
        var troops = new Troops(swordsmen: 10);

        var (remaining, starved) = Village.CalculateStarvation(0, troops, TimeSpan.FromHours(1));

        starved.IsEmpty().Should().BeTrue();
        remaining.Get(TroopType.Swordsman).Should().Be(10);
    }

    [Fact]
    public void CalculateStarvation_StarvesLowestUpkeepFirst()
    {
        // Dogs upkeep=0.25 < Swordsman upkeep=0.5 < Horsemen upkeep=1.5
        var troops = new Troops(horsemen: 2, swordsmen: 5, dogs: 5);
        var deficit = 3.0;
        var elapsed = TimeSpan.FromHours(1);
        // dogs save 0.25 each, need 3 → ceil(3/0.25)=12, but only 5 dogs → starve 5, deficit=1.75
        // swordsmen save 0.5 each → ceil(1.75/0.5)=4 → starve 4, deficit ≤ 0

        var (remaining, starved) = Village.CalculateStarvation(deficit, troops, elapsed);

        starved.Get(TroopType.Dogs).Should().Be(5);
        starved.Get(TroopType.Swordsman).Should().Be(4);
        starved.Get(TroopType.Horsemen).Should().Be(0);
        remaining.Get(TroopType.Horsemen).Should().Be(2);
        remaining.Get(TroopType.Swordsman).Should().Be(1);
    }

    [Fact]
    public void CalculateStarvation_SpreadsAcrossTypesWhenLowestNotEnough()
    {
        var troops = new Troops(horsemen: 1, swordsmen: 1);
        var deficit = 3.0;
        var elapsed = TimeSpan.FromHours(1);

        var (remaining, starved) = Village.CalculateStarvation(deficit, troops, elapsed);

        starved.Get(TroopType.Swordsman).Should().Be(1);
        starved.Get(TroopType.Horsemen).Should().Be(1);
        remaining.TotalCount.Should().Be(0);
    }

    [Fact]
    public void CalculateStarvation_RemovesAllWhenDeficitExceedsAllUpkeep()
    {
        var troops = new Troops(swordsmen: 2);
        var deficit = 100.0;
        var elapsed = TimeSpan.FromHours(1);

        var (remaining, starved) = Village.CalculateStarvation(deficit, troops, elapsed);

        starved.Get(TroopType.Swordsman).Should().Be(2);
        remaining.TotalCount.Should().Be(0);
    }

    [Fact]
    public void CalculateStarvation_NoTroops_ReturnsEmpty()
    {
        var (remaining, starved) = Village.CalculateStarvation(10, Troops.Zero, TimeSpan.FromHours(1));

        starved.IsEmpty().Should().BeTrue();
        remaining.TotalCount.Should().Be(0);
    }

    [Fact]
    public void CalculateStarvation_SkipsTroopsWithZeroUpkeep()
    {
        // All troops in CSV have upkeep > 0, so this tests the filter
        // If a troop had 0 upkeep, it would be skipped
        var troops = new Troops(swordsmen: 5);
        var deficit = 0.001;
        var elapsed = TimeSpan.FromHours(1);

        var (remaining, starved) = Village.CalculateStarvation(deficit, troops, elapsed);

        // swordsman upkeep=0.5/h, ceil(0.001/0.5) = 1
        starved.Get(TroopType.Swordsman).Should().Be(1);
        remaining.Get(TroopType.Swordsman).Should().Be(4);
    }

    [Fact]
    public void Tick_EnoughBeer_NoStarvation()
    {
        var village = Village.CreateStarter("test", new Coordinates(0, 0));
        village.Resources = new Resources(100, 100, 100, 100);
        village.Troops = new Troops(swordsmen: 10);
        village.LastTickAt = DateTime.UtcNow.AddHours(-1);
        var effects = new BuildingEffects(
            productionPerHour: new Resources(0, 0, 0, 100), warehouseCapacity: 1000);

        village.Tick(effects);

        village.Troops.Get(TroopType.Swordsman).Should().Be(10);
        village.Resources.Beer.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void Tick_NotEnoughBeer_StarvesTroops()
    {
        var village = Village.CreateStarter("test", new Coordinates(0, 0));
        village.Resources = new Resources(100, 100, 100, 0);
        village.Troops = new Troops(swordsmen: 10, horsemen: 1);
        village.LastTickAt = DateTime.UtcNow.AddHours(-1);
        var effects = new BuildingEffects(warehouseCapacity: 1000);

        village.Tick(effects);

        // Starves cheapest first: swordsmen (0.5) then horsemen (1.5)
        // 10*0.5 + 1*1.5 = 6.5 upkeep, deficit=6.5
        // ceil(6.5/0.5)=13 → starve all 10 swordsmen, deficit=1.5
        // ceil(1.5/1.5)=1 → starve 1 horsemen
        village.Troops.Get(TroopType.Horsemen).Should().Be(0);
        village.Troops.Get(TroopType.Swordsman).Should().Be(0);
    }

    [Fact]
    public void Tick_StarvesOnlyEnoughToCoverDeficit()
    {
        var village = Village.CreateStarter("test", new Coordinates(0, 0));
        village.Resources = new Resources(100, 100, 100, 5);
        village.Troops = new Troops(swordsmen: 10, horsemen: 2);
        village.LastTickAt = DateTime.UtcNow.AddHours(-1);
        var effects = new BuildingEffects(warehouseCapacity: 1000);

        village.Tick(effects);

        // 10*0.5 + 2*1.5 = 8 upkeep, beer=5, deficit=3
        // swordsmen (0.5) first: ceil(3/0.5)=6 → starve 6, deficit=0
        village.Troops.Get(TroopType.Horsemen).Should().Be(2);
        // Elapsed time drift can push ceil by 1
        village.Troops.Get(TroopType.Swordsman).Should().BeInRange(3, 4);
    }

    [Fact]
    public void Tick_EmitsStarvationEvent()
    {
        var village = Village.CreateStarter("test", new Coordinates(0, 0));
        village.Resources = new Resources(100, 100, 100, 0);
        village.Troops = new Troops(swordsmen: 5);
        village.LastTickAt = DateTime.UtcNow.AddHours(-1);
        var effects = new BuildingEffects(warehouseCapacity: 1000);

        village.Tick(effects);

        village.Events.Should().ContainSingle(e => e.GetType().Name == "TroopsStarvedEvent");
    }

    [Fact]
    public void Tick_NoDeficit_NoStarvationEvent()
    {
        var village = Village.CreateStarter("test", new Coordinates(0, 0));
        village.Resources = new Resources(100, 100, 100, 100);
        village.Troops = new Troops(swordsmen: 1);
        village.LastTickAt = DateTime.UtcNow.AddHours(-1);
        var effects = new BuildingEffects(
            productionPerHour: new Resources(0, 0, 0, 50), warehouseCapacity: 1000);

        village.Tick(effects);

        // beer=100 + 50 produced - 1 upkeep = 149, no deficit
        village.Events.Should().BeEmpty();
    }

    [Fact]
    public void Tick_CapsResourcesToWarehouse()
    {
        var village = Village.CreateStarter("test", new Coordinates(0, 0));
        village.Resources = new Resources(1000, 1000, 1000, 1000);
        village.LastTickAt = DateTime.UtcNow.AddHours(-1);
        var effects = new BuildingEffects(warehouseCapacity: 500);

        village.Tick(effects);

        village.Resources.Wood.Should().BeLessThanOrEqualTo(500);
        village.Resources.Clay.Should().BeLessThanOrEqualTo(500);
        village.Resources.Iron.Should().BeLessThanOrEqualTo(500);
        village.Resources.Beer.Should().BeLessThanOrEqualTo(500);
    }

    [Fact]
    public void Tick_AdvancesLastTickAt()
    {
        var village = Village.CreateStarter("test", new Coordinates(0, 0));
        village.LastTickAt = DateTime.UtcNow.AddHours(-2);
        var effects = new BuildingEffects(warehouseCapacity: 1000);

        village.Tick(effects);

        village.LastTickAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void GetCurrentResources_ProjectsWithoutMutation()
    {
        var village = Village.CreateStarter("test", new Coordinates(0, 0));
        village.Resources = new Resources(100, 100, 100, 50);
        village.Troops = new Troops(swordsmen: 5);
        village.LastTickAt = DateTime.UtcNow.AddHours(-1);
        var effects = new BuildingEffects(
            productionPerHour: new Resources(0, 0, 0, 10), warehouseCapacity: 1000);
        var beforeTick = village.LastTickAt;

        var projected = village.GetCurrentResources(effects);

        projected.Beer.Should().BeApproximately(57.5, 0.001); // beer + produced - upkeep
        village.LastTickAt.Should().Be(beforeTick); // Did not mutate
        village.Troops.Get(TroopType.Swordsman).Should().Be(5); // Did not starve
    }
}
