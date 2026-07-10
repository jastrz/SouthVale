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

public class CancelTrainOrderHandlerTests
{
    private readonly IVillageRepository _repo;
    private readonly IJobScheduler _scheduler;
    private readonly IPlayerRepository _playerRepo;
    private readonly CancelTrainOrderCommandHandler _handler;
    private const string PlayerUserId = "test-user";

    public CancelTrainOrderHandlerTests()
    {
        _repo = Substitute.For<IVillageRepository>();
        _scheduler = Substitute.For<IJobScheduler>();
        _playerRepo = Substitute.For<IPlayerRepository>();
        _handler = new CancelTrainOrderCommandHandler(_repo, _scheduler, _playerRepo);
    }

    [Fact]
    public async Task Success_RefundsResources_RemovesOrder()
    {
        var village = Village.CreateStarter("test", new(0, 0));
        var order = CreateOrder(TroopType.Archer, 5, TimeSpan.FromSeconds(10), DateTime.UtcNow);
        order.JobId = "job-1";
        village.TrainOrders.Add(order);
        village.Resources = Resources.Zero;
        SetupPlayerOwns(village);
        _repo.GetWithTrainOrdersAsync(order.Id, CancellationToken.None).Returns(village);

        var result = await _handler.Handle(new(order.Id, PlayerUserId), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        village.TrainOrders.Should().BeEmpty();
        var cost = TroopsConfig.Get(TroopType.Archer).TrainingCost;
        village.Resources.Should().BeEquivalentTo(cost.Multiply(5));
        _scheduler.Received(1).DeleteJob("job-1");
        await _repo.Received(2).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task PartialRefund_WhenPartiallyCompleted()
    {
        var village = Village.CreateStarter("test", new(0, 0));
        var order = CreateOrder(TroopType.Archer, 10, TimeSpan.FromSeconds(10), DateTime.UtcNow);
        order.Completed = 4;
        order.JobId = "job-1";
        village.TrainOrders.Add(order);
        village.Resources = Resources.Zero;
        SetupPlayerOwns(village);
        _repo.GetWithTrainOrdersAsync(order.Id, CancellationToken.None).Returns(village);

        var result = await _handler.Handle(new(order.Id, PlayerUserId), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        var cost = TroopsConfig.Get(TroopType.Archer).TrainingCost;
        village.Resources.Should().BeEquivalentTo(cost.Multiply(6));
        _scheduler.Received(1).DeleteJob("job-1");
    }

    [Fact]
    public async Task CancelsNonLastOrder_UpdatesTimes()
    {
        var t0 = DateTime.UtcNow;
        var village = Village.CreateStarter("test", new(0, 0));
        var order1 = CreateOrder(TroopType.Archer, 5, TimeSpan.FromSeconds(10), t0);
        order1.CompletesAt = t0.AddSeconds(50);
        var order2 = CreateOrder(TroopType.Swordsman, 10, TimeSpan.FromSeconds(30), t0.AddSeconds(50));
        order2.CompletesAt = t0.AddSeconds(350);
        var order3 = CreateOrder(TroopType.Archer, 5, TimeSpan.FromSeconds(10), t0.AddSeconds(350));
        order3.CompletesAt = t0.AddSeconds(400);
        village.TrainOrders.Add(order1);
        village.TrainOrders.Add(order2);
        village.TrainOrders.Add(order3);
        SetupPlayerOwns(village);
        _repo.GetWithTrainOrdersAsync(order2.Id, CancellationToken.None).Returns(village);

        var result = await _handler.Handle(new(order2.Id, PlayerUserId), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        order1.StartsAt.Should().Be(t0);
        order1.CompletesAt.Should().Be(t0.AddSeconds(50));
        order3.StartsAt.Should().Be(t0.AddSeconds(50));
        order3.CompletesAt.Should().Be(t0.AddSeconds(100));
    }

    [Fact]
    public async Task CancelHeadOrder_RemainingShiftsUp_NewHeadStartsAtUtcNow()
    {
        var village = Village.CreateStarter("test", new(0, 0));
        var head = CreateOrder(TroopType.Archer, 5, TimeSpan.FromSeconds(10), DateTime.UtcNow.AddMinutes(-5));
        head.CompletesAt = DateTime.UtcNow.AddMinutes(-4);
        head.JobId = "job-1";
        var next = CreateOrder(TroopType.Swordsman, 10, TimeSpan.FromSeconds(30), DateTime.UtcNow.AddMinutes(5));
        next.CompletesAt = DateTime.UtcNow.AddMinutes(10);
        next.JobId = "job-2";
        village.TrainOrders.Add(head);
        village.TrainOrders.Add(next);
        SetupPlayerOwns(village);
        _repo.GetWithTrainOrdersAsync(head.Id, CancellationToken.None).Returns(village);

        var before = DateTime.UtcNow;
        var result = await _handler.Handle(new(head.Id, PlayerUserId), CancellationToken.None);
        var after = DateTime.UtcNow;

        result.Succeeded.Should().BeTrue();
        village.TrainOrders.Should().ContainSingle(o => o.Id == next.Id);
        next.StartsAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        next.CompletesAt.Should().BeCloseTo(next.StartsAt.AddSeconds(300), TimeSpan.FromSeconds(1));
        _scheduler.Received(1).DeleteJob("job-1");
        _scheduler.Received(1).DeleteJob("job-2");
        _scheduler.Received(1).ScheduleTrainOrderResolution(next.Id, Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task CancelOnlyOrder_NoRescheduling()
    {
        var village = Village.CreateStarter("test", new(0, 0));
        var order = CreateOrder(TroopType.Archer, 5, TimeSpan.FromSeconds(10), DateTime.UtcNow);
        order.JobId = "job-1";
        village.TrainOrders.Add(order);
        village.Resources = Resources.Zero;
        SetupPlayerOwns(village);
        _repo.GetWithTrainOrdersAsync(order.Id, CancellationToken.None).Returns(village);

        var result = await _handler.Handle(new(order.Id, PlayerUserId), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        village.TrainOrders.Should().BeEmpty();
        var cost = TroopsConfig.Get(TroopType.Archer).TrainingCost;
        village.Resources.Should().BeEquivalentTo(cost.Multiply(5));
        _scheduler.Received(1).DeleteJob("job-1");
        _scheduler.DidNotReceive().ScheduleTrainOrderResolution(Arg.Any<Guid>(), Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task OrderNotFound_Returns404()
    {
        _repo.GetWithTrainOrdersAsync(Arg.Any<Guid>(), CancellationToken.None)
            .Returns((Village?)null);

        var result = await _handler.Handle(new(Guid.NewGuid(), PlayerUserId), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    private void SetupPlayerOwns(Village village)
    {
        village.PlayerId = Guid.NewGuid();
        var player = Substitute.For<Player>();
        player.Id = village.PlayerId;
        _playerRepo.GetByUserIdAsync(PlayerUserId, Arg.Any<CancellationToken>()).Returns(player);
    }

    private static TrainOrder CreateOrder(TroopType type, int amount, TimeSpan timePerUnit, DateTime startedAt)
    {
        var order = TrainOrder.Create(type, amount, timePerUnit, startedAt);
        order.Id = Guid.NewGuid();
        return order;
    }
}
