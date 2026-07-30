using MediatR;
using NSubstitute;
using Xunit;
using TownManager.Application.Interfaces;
using TownManager.Application.Map.Services;
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
    private readonly IPlayerRepository _playerRepo;
    private readonly IMediator _mediator;
    private readonly BarbarianTickService _service;
    private static readonly Player PlayerWithCap = new()
    {
        Username = "test",
        Villages =
        [
            new Village { Troops = new Troops(200, 200, 0, 100, 0, 0) },    //  500 <- max single village
            new Village { Troops = new Troops(100, 100, 0, 50, 50, 10) },   //  310
            new Village { Troops = new Troops(10, 10) },                     //   20
        ]
    };

    private CancellationToken Ct => TestContext.Current.CancellationToken;

    public BarbarianTickServiceTests()
    {
        _repo = Substitute.For<IVillageRepository>();
        _playerRepo = Substitute.For<IPlayerRepository>();
        _playerRepo.GetAllPlayersWithTroopDataAsync(Arg.Any<CancellationToken>()).Returns([PlayerWithCap]);
        _mediator = Substitute.For<IMediator>();
        var mapService = Substitute.For<IMapService>();
        mapService.GetFreeTilesAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns([]);
        _service = new BarbarianTickService(_repo, _playerRepo, mapService, _mediator);

        _repo.SaveChangesAsync(Ct).ReturnsForAnyArgs(Task.CompletedTask);
    }

    [Fact]
    public async Task ExecuteAsync_SkipsBarbarianWithNoTroops()
    {
        var village = MakeBarbarian(new Troops(0, 0));
        _repo.GetBarbarianVillagesAsync(Guid.Empty, Ct).ReturnsForAnyArgs([village]);

        await _service.ExecuteAsync(Ct);

        await _mediator.DidNotReceiveWithAnyArgs().Send(Arg.Any<CreateAttackOrderCommand>(), Ct);
    }

    [Fact]
    public async Task TryAttack_SendsHalfTroopsRoundedUp()
    {
        var village = MakeBarbarian(new Troops(1, 1, 0, 3, 2, 1));
        var target = MakeTarget(new Troops(50, 50));
        _repo.GetBarbarianVillagesAsync(Guid.Empty, Ct).ReturnsForAnyArgs([village]);
        _repo.GetForMapWithinRadius(default, default, Ct).ReturnsForAnyArgs([target]);

        await _service.ExecuteAsync(Ct);

        await _mediator.Received(1).Send(
            Arg.Is<CreateAttackOrderCommand>(c =>
                c.VillageId == village.Id &&
                c.Troops.Count == 5 &&
                c.Troops.Any(t => t.TroopType == TroopType.Swordsman && t.Count == 1) &&
                c.Troops.Any(t => t.TroopType == TroopType.Archer && t.Count == 1) &&
                c.Troops.Any(t => t.TroopType == TroopType.Dogs && t.Count == 2) &&
                c.Troops.Any(t => t.TroopType == TroopType.Horsemen && t.Count == 1) &&
                c.Troops.Any(t => t.TroopType == TroopType.LlamaRiders && t.Count == 1)), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TryAttack_SkipsWhenOnCooldown()
    {
        var village = MakeBarbarian(new Troops(5, 5));
        village.LastAttackAt = DateTime.UtcNow;
        _repo.GetBarbarianVillagesAsync(Guid.Empty, Ct).ReturnsForAnyArgs([village]);

        await _service.ExecuteAsync(Ct);

        await _mediator.DidNotReceiveWithAnyArgs().Send(Arg.Any<CreateAttackOrderCommand>(), Ct);
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
        _repo.GetBarbarianVillagesAsync(Guid.Empty, Ct).ReturnsForAnyArgs([village]);

        await _service.ExecuteAsync(Ct);

        await _mediator.DidNotReceiveWithAnyArgs().Send(Arg.Any<CreateAttackOrderCommand>(), Ct);
    }

    [Fact]
    public async Task TryAttack_SkipsWhenNoTargets()
    {
        var village = MakeBarbarian(new Troops(5, 5));
        _repo.GetBarbarianVillagesAsync(Guid.Empty,Ct).ReturnsForAnyArgs([village]);
        _repo.GetForMapWithinRadius(default, default, Ct).ReturnsForAnyArgs(Array.Empty<Village>());

        await _service.ExecuteAsync(Ct);

        await _mediator.DidNotReceiveWithAnyArgs().Send(Arg.Any<CreateAttackOrderCommand>(),Ct);
    }

    [Fact]
    public async Task AutoTrain_SkipsWhenZeroResources()
    {
        var village = MakeBarbarian(new Troops(0, 0));
        village.Resources = new Resources(0, 0, 0, 0);
        _repo.GetBarbarianVillagesAsync(Guid.Empty, Ct).ReturnsForAnyArgs([village]);

        await _service.ExecuteAsync(Ct);

        await _mediator.DidNotReceiveWithAnyArgs().Send(Arg.Any<CreateTrainOrderCommand>(), Ct);
    }

    [Fact]
    public async Task AutoTrain_TrainsFewerThanFullDeficitWhenResourcesLimited()
    {
        var village = MakeBarbarian(new Troops(0, 0));
        village.Resources = new Resources(100, 100, 100, 100);
        _repo.GetBarbarianVillagesAsync(Guid.Empty, Ct).ReturnsForAnyArgs([village]);

        await _service.ExecuteAsync(Ct);

        await _mediator.Received(1).Send(
            Arg.Is<CreateTrainOrderCommand>(c =>
                c.VillageId == village.Id &&
                c.Orders.Sum(o => o.Count) < BarbarianConfig.MaxTroops.TotalCount),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AutoTrain_SkipsWhenAtMaxTroops()
    {
        // Global cap = 1000 > MaxTroops (770). Per-type MaxTroops must be binding.
        var strongPlayer = new Player
        {
            Username = "strong",
            Villages = [new Village { Troops = new Troops(500, 500) }] // 1000 total → cap=1000
        };
        _playerRepo.GetAllPlayersWithTroopDataAsync(Arg.Any<CancellationToken>()).Returns([strongPlayer]);
        var mapService = Substitute.For<IMapService>();
        mapService.GetFreeTilesAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns([]);
        var freshService = new BarbarianTickService(_repo, _playerRepo, mapService, _mediator);

        var village = MakeBarbarian(new Troops(
            BarbarianConfig.MaxTroops.Get(TroopType.Swordsman),
            BarbarianConfig.MaxTroops.Get(TroopType.Archer),
            0,
            BarbarianConfig.MaxTroops.Get(TroopType.Dogs),
            BarbarianConfig.MaxTroops.Get(TroopType.Horsemen),
            BarbarianConfig.MaxTroops.Get(TroopType.LlamaRiders)));
        _repo.GetBarbarianVillagesAsync(Guid.Empty, Ct).ReturnsForAnyArgs([village]);

        await freshService.ExecuteAsync(Ct);

        await _mediator.DidNotReceiveWithAnyArgs().Send(Arg.Any<CreateTrainOrderCommand>(), Ct);
    }

    [Fact]
    public async Task AutoTrain_SkipsWhenVillageExceedsStrongestSingleVillage()
    {
        // PlayerWithCap max single village = 500. Global cap = 500.
        // MaxTroops per-type: 200+200+300+50+20 = 770. Barbarian at 505 exceeds
        // global cap but each type stays below per-type limit → global cap is binding.
        var village = MakeBarbarian(new Troops(150, 150, 0, 150, 40, 15)); // 505 total, all within per-type limits
        _repo.GetBarbarianVillagesAsync(Guid.Empty, Ct).ReturnsForAnyArgs([village]);

        await _service.ExecuteAsync(Ct);

        await _mediator.DidNotReceiveWithAnyArgs().Send(Arg.Any<CreateTrainOrderCommand>(), Ct);
    }

    [Fact]
    public async Task AutoTrain_TrainsWhenVillageBelowStrongestSingleVillage()
    {
        var village = MakeBarbarian(new Troops(50, 50, 0, 50, 10, 5)); // 165, below cap=500
        _repo.GetBarbarianVillagesAsync(Guid.Empty, Ct).ReturnsForAnyArgs([village]);

        await _service.ExecuteAsync(Ct);

        await _mediator.Received(1).Send(Arg.Any<CreateTrainOrderCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AutoTrain_GlobalCapBindsLowerThanMaxTroops()
    {
        // Strongest player village: 100 troops. Global cap = 100.
        // MaxTroops per-type caps = 770 total. Global cap must bind first.
        // Barbarian at 55 with tons of resources should train at most 45 more.
        var weakPlayer = new Player
        {
            Username = "weak",
            Villages = [new Village { Troops = new Troops(50, 50, 0, 0, 0, 0) }] // 100 total → cap=100
        };
        _playerRepo.GetAllPlayersWithTroopDataAsync(Arg.Any<CancellationToken>()).Returns([weakPlayer]);
        var mapService = Substitute.For<IMapService>();
        mapService.GetFreeTilesAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns([]);
        var freshService = new BarbarianTickService(_repo, _playerRepo, mapService, _mediator);

        var village = MakeBarbarian(new Troops(15, 15, 0, 20, 0, 5)); // 55 total, leaving 45 headroom
        village.Resources = new Resources(5000, 5000, 5000, 5000); // enough to train well past cap
        _repo.GetBarbarianVillagesAsync(Guid.Empty, Ct).ReturnsForAnyArgs([village]);

        await freshService.ExecuteAsync(Ct);

        await _mediator.Received(1).Send(
            Arg.Is<CreateTrainOrderCommand>(c =>
                c.VillageId == village.Id &&
                c.Orders.Sum(o => o.Count) <= 45), // global cap limits, not resources or per-type
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AutoBuild_SendsBuildOrderWhenBelowMax()
    {
        var village = MakeBarbarian(new Troops(0, 0));
        _repo.GetBarbarianVillagesAsync(Guid.Empty, Ct).ReturnsForAnyArgs([village]);

        await _service.ExecuteAsync(Ct);

        foreach (var bt in BarbarianConfig.StartingBuildings.Keys)
        {
            await _mediator.Received(1).Send(
                Arg.Is<CreateBuildOrderCommand>(c => c.VillageId == village.Id && c.BuildingType == bt), Arg.Any<CancellationToken>());
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
        _repo.GetBarbarianVillagesAsync(Guid.Empty, Ct).ReturnsForAnyArgs([village]);

        await _service.ExecuteAsync(Ct);

        await _mediator.DidNotReceiveWithAnyArgs().Send(Arg.Any<CreateBuildOrderCommand>(), Ct);
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
