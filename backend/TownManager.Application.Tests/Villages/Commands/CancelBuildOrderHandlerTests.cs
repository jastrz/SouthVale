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

public class CancelBuildOrderHandlerTests
{
    private readonly IVillageRepository _repo;
    private readonly IJobScheduler _scheduler;
    private readonly CancelBuildOrderCommandHandler _handler;

    public CancelBuildOrderHandlerTests()
    {
        _repo = Substitute.For<IVillageRepository>();
        _scheduler = Substitute.For<IJobScheduler>();
        _handler = new CancelBuildOrderCommandHandler(_repo, _scheduler);
    }

    [Fact]
    public async Task Success_RefundsResources_RemovesOrder()
    {
        var village = Village.CreateStarter("test", new(0, 0));
        var order = CreateOrder(BuildingType.IronMine, 2, TimeSpan.FromMinutes(5));
        order.JobId = "job-1";
        village.BuildOrders.Add(order);
        village.Resources = Resources.Zero;
        _repo.GetWithBuildingsAndOrdersAsync(order.Id, CancellationToken.None).Returns(village);

        var result = await _handler.Handle(new(order.Id), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        village.BuildOrders.Should().BeEmpty();
        village.Resources.Should().BeEquivalentTo(
            BuildingConfig.Get(BuildingType.IronMine, 2).UpgradeCost);
        _scheduler.Received(1).DeleteJob("job-1");
        await _repo.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task CancelsLatestForBuildingType()
    {
        var village = Village.CreateStarter("test", new(0, 0));
        var order1 = CreateOrder(BuildingType.IronMine, 2, TimeSpan.FromMinutes(5));
        var order2 = CreateOrder(BuildingType.IronMine, 3, TimeSpan.FromMinutes(10));
        village.BuildOrders.Add(order1);
        village.BuildOrders.Add(order2);
        _repo.GetWithBuildingsAndOrdersAsync(order2.Id, CancellationToken.None).Returns(village);

        var result = await _handler.Handle(new(order2.Id), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        village.BuildOrders.Should().ContainSingle(o => o.Id == order1.Id);
    }

    [Fact]
    public async Task Fails_WhenNotLatestForBuildingType()
    {
        var village = Village.CreateStarter("test", new(0, 0));
        var order1 = CreateOrder(BuildingType.IronMine, 2, TimeSpan.FromMinutes(5));
        var order2 = CreateOrder(BuildingType.IronMine, 3, TimeSpan.FromMinutes(10));
        village.BuildOrders.Add(order1);
        village.BuildOrders.Add(order2);
        _repo.GetWithBuildingsAndOrdersAsync(order1.Id, CancellationToken.None).Returns(village);

        var result = await _handler.Handle(new(order1.Id), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task UpdatesTimesOfRemainingOrders()
    {
        var t0 = DateTime.UtcNow;
        var village = Village.CreateStarter("test", new(0, 0));
        var iron2 = CreateOrder(BuildingType.IronMine, 2, TimeSpan.FromMinutes(5));
        iron2.StartsAt = t0;
        iron2.CompletesAt = t0.AddMinutes(5);
        var iron3 = CreateOrder(BuildingType.IronMine, 3, TimeSpan.FromMinutes(10));
        iron3.StartsAt = t0.AddMinutes(5);
        iron3.CompletesAt = t0.AddMinutes(15);
        var wood2 = CreateOrder(BuildingType.WoodCutter, 2, TimeSpan.FromMinutes(10));
        wood2.StartsAt = t0.AddMinutes(15);
        wood2.CompletesAt = t0.AddMinutes(25);
        village.BuildOrders.Add(iron2);
        village.BuildOrders.Add(iron3);
        village.BuildOrders.Add(wood2);
        _repo.GetWithBuildingsAndOrdersAsync(iron3.Id, CancellationToken.None).Returns(village);

        await _handler.Handle(new(iron3.Id), CancellationToken.None);

        wood2.StartsAt.Should().Be(t0.AddMinutes(5));
        wood2.CompletesAt.Should().Be(t0.AddMinutes(15));
        iron2.StartsAt.Should().Be(t0);
        iron2.CompletesAt.Should().Be(t0.AddMinutes(5));
    }

    [Fact]
    public async Task OrderNotFound_Returns404()
    {
        _repo.GetWithBuildingsAndOrdersAsync(Arg.Any<Guid>(), CancellationToken.None)
            .Returns((Village?)null);

        var result = await _handler.Handle(new(Guid.NewGuid()), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    private static BuildOrder CreateOrder(BuildingType type, int level, TimeSpan duration)
    {
        var order = BuildOrder.Create(type, level, duration);
        order.Id = Guid.NewGuid();
        return order;
    }
}
