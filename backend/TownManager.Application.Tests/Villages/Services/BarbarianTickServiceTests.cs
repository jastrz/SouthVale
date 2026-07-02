using MediatR;
using NSubstitute;
using Xunit;
using TownManager.Application.Interfaces;
using TownManager.Application.Villages.Commands;
using TownManager.Application.Villages.Services;
using TownManager.Domain.Config;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;

namespace TownManager.Application.Tests.Villages.Services;

public class BarbarianTickServiceTests
{
    private readonly IVillageRepository _repo;
    private readonly IMediator _mediator;
    private readonly BarbarianTickService _service;

    public BarbarianTickServiceTests()
    {
        _repo = Substitute.For<IVillageRepository>();
        _mediator = Substitute.For<IMediator>();
        _service = new BarbarianTickService(_repo, _mediator);

        // Replenish is called on every tick; keep it out of the way.
        _repo.GetAllCoordinatesAsync(CancellationToken.None).ReturnsForAnyArgs(new List<Coordinates>());
        _repo.SaveChangesAsync(CancellationToken.None).ReturnsForAnyArgs(Task.CompletedTask);
    }

    [Fact]
    public async Task ExecuteAsync_SkipsBarbarianWithNoTroops()
    {
        var village = MakeBarbarian(new Troops(0, 0));
        _repo.GetBarbarianVillagesAsync(Guid.Empty).ReturnsForAnyArgs([village]);

        await _service.ExecuteAsync(CancellationToken.None);

        await _mediator.DidNotReceiveWithAnyArgs().Send(Arg.Any<CreateAttackOrderCommand>());
    }

    [Fact]
    public async Task TryAttack_SendsHalfTroopsRoundedUp()
    {
        var village = MakeBarbarian(new Troops(1, 1));
        var target = MakeTarget(new Troops(10, 10));
        _repo.GetBarbarianVillagesAsync(Guid.Empty).ReturnsForAnyArgs([village]);
        _repo.GetForMapWithinRadius(default, default).ReturnsForAnyArgs([target]);

        await _service.ExecuteAsync(CancellationToken.None);

        await _mediator.Received(1).Send(
            Arg.Is<CreateAttackOrderCommand>(c =>
                c.VillageId == village.Id &&
                c.Troops.Count == 2 &&
                c.Troops.Any(t => t.TroopType == TroopType.Swordsman && t.Count == 1) &&
                c.Troops.Any(t => t.TroopType == TroopType.Archer && t.Count == 1)));
    }

    [Fact]
    public async Task TryAttack_SkipsWhenOnCooldown()
    {
        var village = MakeBarbarian(new Troops(5, 5));
        village.LastAttackAt = DateTime.UtcNow;
        _repo.GetBarbarianVillagesAsync(Guid.Empty).ReturnsForAnyArgs([village]);

        await _service.ExecuteAsync(CancellationToken.None);

        await _mediator.DidNotReceiveWithAnyArgs().Send(Arg.Any<CreateAttackOrderCommand>());
    }

    [Fact]
    public async Task TryAttack_SkipsWhenHasOutgoingAttack()
    {
        var village = MakeBarbarian(new Troops(5, 5));
        village.TroopMovements.Add(new TroopMovement
        {
            Type = MovementType.Attack,
            Status = MovementStatus.InFlight,
        });
        _repo.GetBarbarianVillagesAsync(Guid.Empty).ReturnsForAnyArgs([village]);

        await _service.ExecuteAsync(CancellationToken.None);

        await _mediator.DidNotReceiveWithAnyArgs().Send(Arg.Any<CreateAttackOrderCommand>());
    }

    [Fact]
    public async Task TryAttack_SkipsWhenNoTargets()
    {
        var village = MakeBarbarian(new Troops(5, 5));
        _repo.GetBarbarianVillagesAsync(Guid.Empty).ReturnsForAnyArgs([village]);
        _repo.GetForMapWithinRadius(default, default).ReturnsForAnyArgs(Array.Empty<Village>());

        await _service.ExecuteAsync(CancellationToken.None);

        await _mediator.DidNotReceiveWithAnyArgs().Send(Arg.Any<CreateAttackOrderCommand>());
    }

    [Fact]
    public async Task AutoTrain_SendsDeficit()
    {
        var village = MakeBarbarian(new Troops(50, 50));
        _repo.GetBarbarianVillagesAsync(Guid.Empty).ReturnsForAnyArgs([village]);

        await _service.ExecuteAsync(CancellationToken.None);

        var maxSwords = BarbarianConfig.MaxTroops.Swordsmen;
        var maxArchers = BarbarianConfig.MaxTroops.Archers;
        await _mediator.Received(1).Send(
            Arg.Is<CreateTrainOrderCommand>(c =>
                c.Orders.Count == 2 &&
                c.Orders.Any(o => o.TroopType == TroopType.Swordsman && o.Count == maxSwords - 50) &&
                c.Orders.Any(o => o.TroopType == TroopType.Archer && o.Count == maxArchers - 50)));
    }

    [Fact]
    public async Task AutoTrain_SkipsWhenAtMax()
    {
        var village = MakeBarbarian(new Troops(
            BarbarianConfig.MaxTroops.Swordsmen,
            BarbarianConfig.MaxTroops.Archers));
        _repo.GetBarbarianVillagesAsync(Guid.Empty).ReturnsForAnyArgs([village]);

        await _service.ExecuteAsync(CancellationToken.None);

        await _mediator.DidNotReceiveWithAnyArgs().Send(Arg.Any<CreateTrainOrderCommand>());
    }

    [Fact]
    public async Task AutoBuild_SendsBuildOrderWhenBelowMax()
    {
        var village = MakeBarbarian(new Troops(0, 0));
        _repo.GetBarbarianVillagesAsync(Guid.Empty).ReturnsForAnyArgs([village]);

        await _service.ExecuteAsync(CancellationToken.None);

        foreach (var bt in BarbarianConfig.StartingBuildings.Keys)
        {
            await _mediator.Received(1).Send(
                Arg.Is<CreateBuildOrderCommand>(c => c.VillageId == village.Id && c.BuildingType == bt));
        }
    }

    [Fact]
    public async Task AutoBuild_SkipsWhenAllAtMax()
    {
        var village = MakeBarbarian(new Troops(0, 0));
        foreach (var bt in BarbarianConfig.StartingBuildings.Keys)
        {
            var building = village.Buildings.First(b => b.Type == bt);
            building.Level = BarbarianConfig.MaxBuildingLevel;
        }
        _repo.GetBarbarianVillagesAsync(Guid.Empty).ReturnsForAnyArgs([village]);

        await _service.ExecuteAsync(CancellationToken.None);

        await _mediator.DidNotReceiveWithAnyArgs().Send(Arg.Any<CreateBuildOrderCommand>());
    }

    private static Village MakeBarbarian(Troops troops)
    {
        var v = new Village
        {
            Id = Guid.NewGuid(),
            Name = "Test Barbarian",
            VillageType = VillageType.Barbarian,
            PlayerId = BarbarianConfig.BarbarianPlayerId,
            Troops = troops,
            Resources = new Resources(1000, 1000, 1000, 1000),
            Coordinates = new(10, 10),
            Buildings = BarbarianConfig.StartingBuildings
                .Select(kv => Building.Create(kv.Key, kv.Value))
                .ToList(),
        };
        return v;
    }

    private static Village MakeTarget(Troops troops) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Target",
        VillageType = VillageType.Player,
        PlayerId = Guid.NewGuid(),
        Troops = troops,
        Coordinates = new(12, 12),
    };
}
