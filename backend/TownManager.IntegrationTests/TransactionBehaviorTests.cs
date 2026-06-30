using System.Net.Http.Json;
using FluentAssertions;

namespace TownManager.IntegrationTests;

[Collection("Integration")]
public class TransactionBehaviorTests : IntegrationTestBase
{
    public TransactionBehaviorTests(PostgresFixture postgres) : base(postgres) { }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task CreateBuildOrder_GoesThroughPipeline()
    {
        await RegisterAsync();
        var villages = await GetVillagesAsync();
        var villageId = villages[0].Id;

        var buildResponse = await Client.PostAsJsonAsync(
            $"/gameplay/village/{villageId}/build",
            new { buildingType = "IronMine" },
            JsonOptions, Ct);
        buildResponse.EnsureSuccessStatusCode();

        var villageResponse = await Client.GetAsync($"/gameplay/village/{villageId}", Ct);
        villageResponse.EnsureSuccessStatusCode();
        var village = await villageResponse.Content.ReadFromJsonAsync<VillageDetailDto>(JsonOptions, cancellationToken: Ct);

        village!.BuildOrders.Should().ContainSingle(o => o.BuildingType == "IronMine");
    }

    [Fact]
    public async Task CancelBuildOrder_GoesThroughPipeline()
    {
        await RegisterAsync();
        var villages = await GetVillagesAsync();
        var villageId = villages[0].Id;

        var buildResponse = await Client.PostAsJsonAsync(
            $"/gameplay/village/{villageId}/build",
            new { buildingType = "IronMine" },
            JsonOptions, Ct);
        buildResponse.EnsureSuccessStatusCode();

        var villageResponse = await Client.GetAsync($"/gameplay/village/{villageId}", Ct);
        villageResponse.EnsureSuccessStatusCode();
        var village = await villageResponse.Content.ReadFromJsonAsync<VillageDetailDto>(JsonOptions, cancellationToken: Ct);
        var orderId = village!.BuildOrders[0].Id;

        var cancelResponse = await Client.PostAsync($"/gameplay/build/{orderId}/cancel", null, Ct);
        cancelResponse.EnsureSuccessStatusCode();

        var villageAfterResponse = await Client.GetAsync($"/gameplay/village/{villageId}", Ct);
        villageAfterResponse.EnsureSuccessStatusCode();
        var villageAfter = await villageAfterResponse.Content.ReadFromJsonAsync<VillageDetailDto>(JsonOptions, cancellationToken: Ct);

        villageAfter!.BuildOrders.Should().BeEmpty();
    }

    [Fact]
    public async Task RegisterAndGetVillages_Works()
    {
        await RegisterAsync();
        var villages = await GetVillagesAsync();
        villages.Should().NotBeEmpty();
    }

    private record VillageDetailDto(
        Guid Id,
        string Name,
        ResourcesDto Resources,
        TroopsDto Troops,
        List<BuildingDto> Buildings,
        List<BuildOrderDto> BuildOrders,
        List<TrainOrderDto> TrainOrders,
        CoordinatesDto Coordinates
    );

    private record ResourcesDto(int Wood, int Clay, int Iron, int Crop);
    private record TroopsDto(int Swordsmen, int Archers, int Settlers);
    private record BuildingDto(Guid Id, string Type, int Level);
    private record BuildOrderDto(Guid Id, string BuildingType, int TargetLevel, DateTime StartsAt, DateTime CompletesAt);
    private record TrainOrderDto(Guid Id, string Type, int Amount, int Completed, DateTime StartsAt, DateTime CompletesAt);
    private record CoordinatesDto(int X, int Y);
}
