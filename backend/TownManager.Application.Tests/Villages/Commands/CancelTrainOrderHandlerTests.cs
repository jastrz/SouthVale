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
    private readonly CancelTrainOrderCommandHandler _handler;

    public CancelTrainOrderHandlerTests()
    {
        _repo = Substitute.For<IVillageRepository>();
        _scheduler = Substitute.For<IJobScheduler>();
        _handler = new CancelTrainOrderCommandHandler(_repo, _scheduler);
    }

    [Fact]
    public async Task Success_RefundsResources_RemovesOrder()
    {
        var village = Village.CreateStarter("test", new(0, 0));
        var order = CreateOrder(TroopType.Archer, 5, TimeSpan.FromSeconds(10), DateTime.UtcNow);
        order.JobId = "job-1";
        village.TrainOrders.Add(order);
        village.Resources = Resources.Zero;
        _repo.GetWithTrainOrdersAsync(order.Id, CancellationToken.None).Returns(village);

        var result = await _handler.Handle(new(order.Id), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        village.TrainOrders.Should().BeEmpty();
        var cost = TroopsConfig.Get(TroopType.Archer).TrainingCost;
        village.Resources.Should().BeEquivalentTo(cost.Multiply(5));
        _scheduler.Received(1).DeleteJob("job-1");
        await _repo.Received(1).SaveChangesAsync(CancellationToken.None);
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
        _repo.GetWithTrainOrdersAsync(order.Id, CancellationToken.None).Returns(village);

        var result = await _handler.Handle(new(order.Id), CancellationToken.None);

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
        _repo.GetWithTrainOrdersAsync(order2.Id, CancellationToken.None).Returns(village);

        var result = await _handler.Handle(new(order2.Id), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        order1.StartedAt.Should().Be(t0); // unchanged
        order1.CompletesAt.Should().Be(t0.AddSeconds(50)); // unchanged
        order3.StartedAt.Should().Be(t0.AddSeconds(50)); // shifted up
        order3.CompletesAt.Should().Be(t0.AddSeconds(100)); // shifted up
    }

    [Fact]
    public async Task OrderNotFound_Returns404()
    {
        _repo.GetWithTrainOrdersAsync(Arg.Any<Guid>(), CancellationToken.None)
            .Returns((Village?)null);

        var result = await _handler.Handle(new(Guid.NewGuid()), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    private static TrainOrder CreateOrder(TroopType type, int amount, TimeSpan timePerUnit, DateTime startedAt)
    {
        var order = TrainOrder.Create(type, amount, timePerUnit, startedAt);
        order.Id = Guid.NewGuid();
        return order;
    }
}
