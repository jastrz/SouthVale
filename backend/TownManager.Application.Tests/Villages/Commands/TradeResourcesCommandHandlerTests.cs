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

public class TradeResourcesCommandHandlerTests
{
    private readonly IVillageRepository _repo;
    private readonly IPlayerRepository _playerRepo;
    private readonly IGameNotificationService _notifications;
    private readonly TradeResourcesCommandHandler _handler;

    public TradeResourcesCommandHandlerTests()
    {
        _repo = Substitute.For<IVillageRepository>();
        _playerRepo = Substitute.For<IPlayerRepository>();
        _notifications = Substitute.For<IGameNotificationService>();
        _handler = new TradeResourcesCommandHandler(_repo, _notifications, _playerRepo);
    }

    [Fact]
    public async Task Success_DeductsGive_AddsReceive()
    {
        var village = CreateVillageWithTradePost(level: 5);
        _repo.GetForCombatAsync(village.Id, CancellationToken.None).Returns(village);
        _playerRepo.GetUserIdByPlayerIdAsync(village.PlayerId, CancellationToken.None).Returns("user1");

        var result = await _handler.Handle(
            new(village.Id, ResourceType.Wood, 100, ResourceType.Clay), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        var effects = BuildingConfig.AggregateEffects(village.Buildings);
        var expectedReceive = (int)Math.Floor(100 * effects.TradeRate);
        village.Resources.Wood.Should().BeApproximately(400, 0.001);
        village.Resources.Clay.Should().BeApproximately(500 + expectedReceive, 0.001);
        await _repo.Received(1).SaveChangesAsync(CancellationToken.None);
        await _notifications.Received(1).VillageUpdatedAsync("user1", village.Id, CancellationToken.None);
    }

    [Fact]
    public async Task VillageNotFound_Returns404()
    {
        _repo.GetForCombatAsync(Arg.Any<Guid>(), CancellationToken.None).Returns((Village?)null);

        var result = await _handler.Handle(
            new(Guid.NewGuid(), ResourceType.Wood, 100, ResourceType.Clay), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task NoTradingPost_ReturnsFailure()
    {
        var village = Village.CreateStarter("test", new Coordinates(0, 0));
        village.Resources = new Resources(1000, 1000, 1000, 1000);
        _repo.GetForCombatAsync(village.Id, CancellationToken.None).Returns(village);

        var result = await _handler.Handle(
            new(village.Id, ResourceType.Wood, 100, ResourceType.Clay), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task SameResource_ReturnsFailure()
    {
        var village = CreateVillageWithTradePost(level: 5);
        _repo.GetForCombatAsync(village.Id, CancellationToken.None).Returns(village);

        var result = await _handler.Handle(
            new(village.Id, ResourceType.Wood, 100, ResourceType.Wood), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task InsufficientResources_ReturnsFailure()
    {
        var village = CreateVillageWithTradePost(level: 5);
        village.Resources = Resources.Zero;
        _repo.GetForCombatAsync(village.Id, CancellationToken.None).Returns(village);

        var result = await _handler.Handle(
            new(village.Id, ResourceType.Wood, 100, ResourceType.Clay), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task ZeroAmount_ReturnsFailure()
    {
        var village = CreateVillageWithTradePost(level: 5);
        _repo.GetForCombatAsync(village.Id, CancellationToken.None).Returns(village);

        var result = await _handler.Handle(
            new(village.Id, ResourceType.Wood, 0, ResourceType.Clay), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task WarehouseCapacityExceeded_ReturnsFailure()
    {
        var village = CreateVillageWithTradePost(level: 5);
        var effects = BuildingConfig.AggregateEffects(village.Buildings);
        village.Resources = new Resources(500, effects.WarehouseCapacity - 10, 500, 500);
        _repo.GetForCombatAsync(village.Id, CancellationToken.None).Returns(village);

        var result = await _handler.Handle(
            new(village.Id, ResourceType.Wood, 100, ResourceType.Clay), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task TradeRateTooLow_ReturnsFailure()
    {
        var village = CreateVillageWithTradePost(level: 1);
        _repo.GetForCombatAsync(village.Id, CancellationToken.None).Returns(village);

        var result = await _handler.Handle(
            new(village.Id, ResourceType.Wood, 1, ResourceType.Clay), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
    }

    private static Village CreateVillageWithTradePost(int level)
    {
        var village = Village.CreateStarter("test", new Coordinates(0, 0));
        village.Buildings.Add(Building.Create(BuildingType.TradePost, level));
        village.Resources = new Resources(500, 500, 500, 500);
        return village;
    }
}
