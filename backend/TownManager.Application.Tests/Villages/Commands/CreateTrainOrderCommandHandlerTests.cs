using FluentAssertions;
using NSubstitute;
using Xunit;
using TownManager.Application.Interfaces;
using TownManager.Application.Villages.Commands;
using TownManager.Domain.Entities;
using TownManager.Domain.Entities.Villages;
using TownManager.Domain.Enums;

namespace TownManager.Application.Tests.Villages.Commands;

public class CreateTrainOrderCommandHandlerTests
{
    private readonly IVillageRepository _repo;
    private readonly IJobScheduler _scheduler;
    private readonly CreateTrainOrderCommandHandler _handler;

    public CreateTrainOrderCommandHandlerTests()
    {
        _repo = Substitute.For<IVillageRepository>();
        _scheduler = Substitute.For<IJobScheduler>();
        _handler = new CreateTrainOrderCommandHandler(_repo, _scheduler);
    }

    [Fact]
    public async Task SingleTroopType_CreatesOneOrder()
    {
        var village = CreateVillage();
        _repo.GetWithActiveOrdersAsync(village.Id, default).Returns(village);

        var result = await _handler.Handle(
            new(village.Id, [new(TroopType.Swordsman, 10)]), default);

        result.Succeeded.Should().BeTrue();
        village.TrainOrders.Should().ContainSingle(o =>
            o.Type == TroopType.Swordsman && o.Amount == 10);
    }

    [Fact]
    public async Task MultipleTypes_CreateSeparateOrders()
    {
        var village = CreateVillage();
        _repo.GetWithActiveOrdersAsync(village.Id, default).Returns(village);

        var result = await _handler.Handle(
            new(village.Id, [
                new(TroopType.Swordsman, 5),
                new(TroopType.Archer, 3),
            ]), default);

        result.Succeeded.Should().BeTrue();
        village.TrainOrders.Should().HaveCount(2);
        village.TrainOrders.Should().Contain(o => o.Type == TroopType.Swordsman && o.Amount == 5);
        village.TrainOrders.Should().Contain(o => o.Type == TroopType.Archer && o.Amount == 3);
    }

    [Fact]
    public async Task NewOrders_ChainAfterExistingQueue()
    {
        var village = CreateVillage();
        var existingOrder = TrainOrder.Create(TroopType.Swordsman, 1, TimeSpan.FromMinutes(10), DateTime.UtcNow);
        village.TrainOrders.Add(existingOrder);
        _repo.GetWithActiveOrdersAsync(village.Id, default).Returns(village);

        var result = await _handler.Handle(
            new(village.Id, [new(TroopType.Archer, 1)]), default);

        result.Succeeded.Should().BeTrue();
        var newOrder = village.TrainOrders.Single(o => o.Type == TroopType.Archer);
        newOrder.StartedAt.Should().Be(existingOrder.CompletesAt);
    }

    [Fact]
    public async Task InsufficientResources_ReturnsFailure()
    {
        var village = CreateVillage();
        village.Resources = new Resources(1, 1, 1, 1);
        _repo.GetWithActiveOrdersAsync(village.Id, default).Returns(village);

        var result = await _handler.Handle(
            new(village.Id, [new(TroopType.Archer, 1000)]), default);

        result.Succeeded.Should().BeFalse();
        village.TrainOrders.Should().BeEmpty();
    }

    [Fact]
    public async Task VillageNotFound_Returns404()
    {
        _repo.GetWithActiveOrdersAsync(Arg.Any<Guid>(), default).Returns((Village?)null);

        var result = await _handler.Handle(
            new(Guid.NewGuid(), [new(TroopType.Swordsman, 1)]), default);

        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Success_SavesAndSchedules()
    {
        var village = CreateVillage();
        _repo.GetWithActiveOrdersAsync(village.Id, default).Returns(village);

        await _handler.Handle(
            new(village.Id, [new(TroopType.Swordsman, 5)]), default);

        await _repo.Received(1).SaveChangesAsync(default);
        _scheduler.Received(1).ScheduleTrainOrderResolution(
            Arg.Any<Guid>(), Arg.Any<TimeSpan>());
    }

    private static Village CreateVillage()
    {
        var village = Village.CreateStarter("test", new Coordinates(0, 0));
        village.Buildings.Clear();
        village.Resources = new Resources(99999, 99999, 99999, 99999);
        return village;
    }
}
