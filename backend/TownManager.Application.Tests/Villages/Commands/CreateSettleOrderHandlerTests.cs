using FluentAssertions;
using NSubstitute;
using Xunit;
using TownManager.Application.Interfaces;
using TownManager.Application.Villages.Commands;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;

namespace TownManager.Application.Tests.Villages.Commands;

public class CreateSettleOrderHandlerTests
{
    private readonly IVillageRepository _repo;
    private readonly IPlayerRepository _playerRepo;
    private readonly IGameNotificationService _notifications;
    private readonly IJobScheduler _scheduler;
    private readonly CreateSettleOrderCommandHandler _handler;

    public CreateSettleOrderHandlerTests()
    {
        _repo = Substitute.For<IVillageRepository>();
        _playerRepo = Substitute.For<IPlayerRepository>();
        _notifications = Substitute.For<IGameNotificationService>();
        _scheduler = Substitute.For<IJobScheduler>();
        _handler = new CreateSettleOrderCommandHandler(_repo, _playerRepo, _notifications, _scheduler);
    }

    [Fact]
    public async Task Success_DeductsSettlerAndCreatesMovement()
    {
        var village = CreateVillageWithSettlers(1);
        var target = new Coordinates(5, 5);
        _repo.GetWithMovementOrdersAsync(village.Id, CancellationToken.None).Returns(village);
        _repo.GetByCoordsAsync(target, CancellationToken.None).Returns((Village?)null);

        var result = await _handler.Handle(new(village.Id, target), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        village.Troops.Get(TroopType.Settler).Should().Be(0);
        village.TroopMovements.Should().ContainSingle(m =>
            m.Type == MovementType.Settle && m.TargetCoordinates == target);
        await _repo.Received(1).SaveChangesAsync(CancellationToken.None);
        _scheduler.Received(1).ScheduleMovementResolution(
            Arg.Any<Guid>(), Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task VillageNotFound_Returns404()
    {
        _repo.GetWithMovementOrdersAsync(Arg.Any<Guid>(), CancellationToken.None).Returns((Village?)null);

        var result = await _handler.Handle(new(Guid.NewGuid(), new(1, 1)), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task NotEnoughSettlers_ReturnsFailure()
    {
        var village = CreateVillageWithSettlers(0);
        _repo.GetWithMovementOrdersAsync(village.Id, CancellationToken.None).Returns(village);

        var result = await _handler.Handle(new(village.Id, new(5, 5)), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        village.Troops.Get(TroopType.Settler).Should().Be(0);
    }

    [Fact]
    public async Task TargetTileOccupied_Returns409()
    {
        var village = CreateVillageWithSettlers(1);
        var target = new Coordinates(5, 5);
        var occupant = Village.CreateStarter("occupant", target);
        _repo.GetWithMovementOrdersAsync(village.Id, CancellationToken.None).Returns(village);
        _repo.GetByCoordsAsync(target, CancellationToken.None).Returns(occupant);

        var result = await _handler.Handle(new(village.Id, target), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        village.Troops.Get(TroopType.Settler).Should().Be(1);
    }

    private static Village CreateVillageWithSettlers(int count)
    {
        var village = Village.CreateStarter("test", new Coordinates(0, 0));
        village.Troops = new Troops(settlers: count);
        return village;
    }
}
