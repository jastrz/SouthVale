using FluentAssertions;
using NSubstitute;
using Xunit;
using TownManager.Application.Interfaces;
using TownManager.Application.Villages.Commands;
using TownManager.Domain.Config;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;

namespace TownManager.Application.Tests.Villages.Commands;

public class CreateBuildOrderHandlerTests
{
    private readonly IVillageRepository _repo;
    private readonly IJobScheduler _scheduler;
    private readonly CreateBuildOrderHandler _handler;

    public CreateBuildOrderHandlerTests()
    {
        _repo = Substitute.For<IVillageRepository>();
        _scheduler = Substitute.For<IJobScheduler>();
        _handler = new CreateBuildOrderHandler(_repo, _scheduler);
    }

    [Fact]
    public async Task FirstOrder_IncrementsFromBuildingLevel()
    {
        var village = CreateVillageWithBuilding(BuildingType.IronMine, level: 1);
        _repo.GetWithActiveOrdersAsync(village.Id, CancellationToken.None).Returns(village);
        _repo.GetMaxBuildOrderTargetAsync(village.Id, BuildingType.IronMine, CancellationToken.None)
            .Returns((int?)null);

        var result = await _handler.Handle(new(village.Id, BuildingType.IronMine), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        village.BuildOrders.Should().ContainSingle(o =>
            o.TargetLevel == 2 && o.BuildingType == BuildingType.IronMine);
    }

    [Fact]
    public async Task SecondOrder_IncrementsFromQueuedTarget()
    {
        var village = CreateVillageWithBuilding(BuildingType.IronMine, level: 1);
        _repo.GetWithActiveOrdersAsync(village.Id, CancellationToken.None).Returns(village);
        _repo.GetMaxBuildOrderTargetAsync(village.Id, BuildingType.IronMine, CancellationToken.None)
            .Returns(2);

        var result = await _handler.Handle(new(village.Id, BuildingType.IronMine), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        village.BuildOrders.Should().ContainSingle(o => o.TargetLevel == 3);
    }

    [Fact]
    public async Task DifferentTypes_ChainIndependently()
    {
        var village = CreateVillageWithBuilding(BuildingType.IronMine, level: 1);
        village.Buildings.Add(Building.Create(BuildingType.ClayPit, 1));
        _repo.GetWithActiveOrdersAsync(village.Id, CancellationToken.None).Returns(village);
        _repo.GetMaxBuildOrderTargetAsync(village.Id, BuildingType.IronMine, CancellationToken.None)
            .Returns(2);
        _repo.GetMaxBuildOrderTargetAsync(village.Id, BuildingType.ClayPit, CancellationToken.None)
            .Returns((int?)null);

        await _handler.Handle(new(village.Id, BuildingType.IronMine), CancellationToken.None);
        var result = await _handler.Handle(new(village.Id, BuildingType.ClayPit), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        village.BuildOrders.Should().ContainSingle(o =>
            o.BuildingType == BuildingType.ClayPit && o.TargetLevel == 2);
    }

    [Fact]
    public async Task InsufficientResources_ReturnsFailure()
    {
        var village = CreateVillageWithBuilding(BuildingType.IronMine, level: 4);
        village.Resources = Resources.Zero;
        _repo.GetWithActiveOrdersAsync(village.Id, CancellationToken.None).Returns(village);
        _repo.GetMaxBuildOrderTargetAsync(village.Id, BuildingType.IronMine, CancellationToken.None)
            .Returns((int?)null);

        var result = await _handler.Handle(new(village.Id, BuildingType.IronMine), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        village.BuildOrders.Should().BeEmpty();
    }

    [Fact]
    public async Task QueueFull_ReturnsFailure()
    {
        var village = CreateVillageWithBuilding(BuildingType.IronMine, level: 1);
        var limit = GameSettings.MaxBuildQueueSize + 1;
        for (var i = 0; i < limit; i++)
            village.BuildOrders.Add(BuildOrder.Create(BuildingType.WoodCutter, 2, TimeSpan.FromMinutes(5)));
        _repo.GetWithActiveOrdersAsync(village.Id, CancellationToken.None).Returns(village);
        _repo.GetMaxBuildOrderTargetAsync(village.Id, BuildingType.IronMine, CancellationToken.None)
            .Returns((int?)null);
        _repo.GetMaxTownHallLevelAsync(village.PlayerId, CancellationToken.None).Returns(1);

        var result = await _handler.Handle(new(village.Id, BuildingType.IronMine), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        village.BuildOrders.Should().HaveCount(limit);
        _scheduler.DidNotReceive().ScheduleBuildOrderResolution(Arg.Any<Guid>(), Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task Bot_IgnoresQueueLimit_WhenFlagEnabled()
    {
        GameSettings.BotsIgnoreQueueSize = true;
        try
        {
            var village = CreateVillageWithBuilding(BuildingType.IronMine, level: 1);
            var limit = GameSettings.MaxBuildQueueSize + 1;
            for (var i = 0; i < limit; i++)
                village.BuildOrders.Add(BuildOrder.Create(BuildingType.WoodCutter, 2, TimeSpan.FromMinutes(5)));
            _repo.GetWithActiveOrdersAsync(village.Id, CancellationToken.None).Returns(village);
            _repo.GetMaxBuildOrderTargetAsync(village.Id, BuildingType.IronMine, CancellationToken.None)
                .Returns((int?)null);
            _repo.GetMaxTownHallLevelAsync(village.PlayerId, CancellationToken.None).Returns(1);

            var result = await _handler.Handle(new(village.Id, BuildingType.IronMine, IsBot: true), CancellationToken.None);

            result.Succeeded.Should().BeTrue();
            village.BuildOrders.Should().HaveCount(limit + 1);
        }
        finally
        {
            GameSettings.BotsIgnoreQueueSize = false;
        }
    }

    [Fact]
    public async Task HigherTownHall_ExtendsQueueLimit()
    {
        var village = CreateVillageWithBuilding(BuildingType.IronMine, level: 1);
        for (var i = 0; i < GameSettings.MaxBuildQueueSize + 1; i++)
            village.BuildOrders.Add(BuildOrder.Create(BuildingType.WoodCutter, 2, TimeSpan.FromMinutes(5)));
        _repo.GetWithActiveOrdersAsync(village.Id, CancellationToken.None).Returns(village);
        _repo.GetMaxBuildOrderTargetAsync(village.Id, BuildingType.IronMine, CancellationToken.None)
            .Returns((int?)null);
        _repo.GetMaxTownHallLevelAsync(village.PlayerId, CancellationToken.None).Returns(2);

        var result = await _handler.Handle(new(village.Id, BuildingType.IronMine), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        village.BuildOrders.Should().HaveCount(GameSettings.MaxBuildQueueSize + 2);
    }

    [Fact]
    public async Task VillageNotFound_Returns404()
    {
        _repo.GetWithActiveOrdersAsync(Arg.Any<Guid>(), CancellationToken.None).Returns((Village?)null);

        var result = await _handler.Handle(new(Guid.NewGuid(), BuildingType.IronMine), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Success_SavesAndSchedules()
    {
        var village = CreateVillageWithBuilding(BuildingType.IronMine, level: 1);
        _repo.GetWithActiveOrdersAsync(village.Id, CancellationToken.None).Returns(village);
        _repo.GetMaxBuildOrderTargetAsync(village.Id, BuildingType.IronMine, CancellationToken.None)
            .Returns((int?)null);

        await _handler.Handle(new(village.Id, BuildingType.IronMine), CancellationToken.None);

        await _repo.Received(2).SaveChangesAsync(CancellationToken.None);
        _scheduler.Received(1).ScheduleBuildOrderResolution(
            Arg.Any<Guid>(), Arg.Any<TimeSpan>());
    }

    private static Village CreateVillageWithBuilding(BuildingType type, int level)
    {
        var village = Village.CreateStarter("test", new Coordinates(0, 0));
        village.Resources = new Resources(99999, 99999, 99999, 99999);
        return village;
    }
}
