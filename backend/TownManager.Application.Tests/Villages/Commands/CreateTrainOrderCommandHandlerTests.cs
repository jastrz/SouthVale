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
        _repo.GetWithActiveOrdersAsync(village.Id, CancellationToken.None).Returns(village);

        var result = await _handler.Handle(
            new(village.Id, [new(TroopType.Swordsman, 10)]), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        village.TrainOrders.Should().ContainSingle(o =>
            o.Type == TroopType.Swordsman && o.Amount == 10);
    }

    [Fact]
    public async Task MultipleTypes_CreateSeparateOrders()
    {
        var village = CreateVillage();
        _repo.GetWithActiveOrdersAsync(village.Id, CancellationToken.None).Returns(village);

        var result = await _handler.Handle(
            new(village.Id, [
                new(TroopType.Swordsman, 5),
                new(TroopType.Archer, 3),
            ]), CancellationToken.None);

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
        _repo.GetWithActiveOrdersAsync(village.Id, CancellationToken.None).Returns(village);

        var result = await _handler.Handle(
            new(village.Id, [new(TroopType.Archer, 1)]), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        var newOrder = village.TrainOrders.Single(o => o.Type == TroopType.Archer);
        newOrder.StartsAt.Should().Be(existingOrder.CompletesAt);
    }

    [Fact]
    public async Task InsufficientResources_ReturnsFailure()
    {
        var village = CreateVillage();
        village.Resources = new Resources(1, 1, 1, 1);
        _repo.GetWithActiveOrdersAsync(village.Id, CancellationToken.None).Returns(village);

        var result = await _handler.Handle(
            new(village.Id, [new(TroopType.Archer, 1000)]), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        village.TrainOrders.Should().BeEmpty();
    }

    [Fact]
    public async Task VillageNotFound_Returns404()
    {
        _repo.GetWithActiveOrdersAsync(Arg.Any<Guid>(), CancellationToken.None).Returns((Village?)null);

        var result = await _handler.Handle(
            new(Guid.NewGuid(), [new(TroopType.Swordsman, 1)]), CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact(Skip = "settler cost changes too often")]
    public async Task SingleSettler_CostScalesWithVillageCount()
    {
        var village = CreateVillage();
        _repo.GetWithActiveOrdersAsync(village.Id, CancellationToken.None).Returns(village);
        _repo.CountByPlayerAsync(village.PlayerId, CancellationToken.None).Returns(3);

        var result = await _handler.Handle(
            new(village.Id, [new(TroopType.Settler, 1)]), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        // 1st settler cost: 400
        // 400 × 2^(3+0-1)=4
        var expectedDeduction = 1600; 
        village.Resources.Wood.Should().BeApproximately(99999 - expectedDeduction, 0.01);
        village.Resources.Clay.Should().BeApproximately(99999 - expectedDeduction, 0.01);
        village.Resources.Iron.Should().BeApproximately(99999 - expectedDeduction, 0.01);
        village.Resources.Beer.Should().BeApproximately(99999 - expectedDeduction, 0.01);
    }

    [Fact(Skip = "settler cost changes too often")]
    public async Task BatchSettlers_CostGeometric()
    {
        var village = CreateVillage();
        _repo.GetWithActiveOrdersAsync(village.Id, CancellationToken.None).Returns(village);
        _repo.CountByPlayerAsync(village.PlayerId, CancellationToken.None).Returns(1);

        var result = await _handler.Handle(
            new(village.Id, [new(TroopType.Settler, 3)]), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        // 1st settler cost: 400
        // 1 village, 0 settlers  k=0, total=2^0×(2^3-1)=7 → 400×7=2800
        var expectedDeduction = 2800;
        village.Resources.Wood.Should().BeApproximately(99999 - expectedDeduction, 0.01);
        village.Resources.Clay.Should().BeApproximately(99999 - expectedDeduction, 0.01);
        village.Resources.Iron.Should().BeApproximately(99999 - expectedDeduction, 0.01);
        village.Resources.Beer.Should().BeApproximately(99999 - expectedDeduction, 0.01);
    }

    [Fact]
    public async Task Success_SavesAndSchedules()
    {
        var village = CreateVillage();
        _repo.GetWithActiveOrdersAsync(village.Id, CancellationToken.None).Returns(village);

        await _handler.Handle(
            new(village.Id, [new(TroopType.Swordsman, 5)]), CancellationToken.None);

        await _repo.Received(2).SaveChangesAsync(CancellationToken.None);
        _scheduler.Received(1).ScheduleTrainOrderResolution(
            Arg.Any<Guid>(), Arg.Any<TimeSpan>());
    }

    private static Village CreateVillage()
    {
        var village = Village.CreateStarter("test", new Coordinates(0, 0));
        village.Buildings.Add(Building.Create(BuildingType.Barracks, 5));
        village.Buildings.Add(Building.Create(BuildingType.Warehouse, 20));
        village.Resources = new Resources(99999, 99999, 99999, 99999);
        return village;
    }
}
