using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TownManager.Application.Dtos;
using TownManager.Application.World;
using TownManager.Infrastructure.Data;
using TownManager.Infrastructure.Persistence;

namespace TownManager.IntegrationTests;

[Collection("Integration")]
public class WorldResetServiceTests : IntegrationTestBase
{
    public WorldResetServiceTests(PostgresFixture postgres) : base(postgres) { }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Reset_KeepsRegisteredUser_ClosesIteration_StartsNext()
    {
        await RegisterAsync();
        (await GetVillagesAsync()).Should().NotBeEmpty();

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await new WorldIterationSeeder(db).SeedAsync();

        var reset = scope.ServiceProvider.GetRequiredService<IWorldResetService>();

        var result = await reset.ResetAsync(Ct);

        result.IterationNumber.Should().Be(2);
        result.PersistedUsers.Should().BeGreaterThanOrEqualTo(1);
        result.RecreatedPlayers.Should().BeGreaterThanOrEqualTo(1);

        var finished = await db.WorldIterations.SingleAsync(i => i.EndedAt != null, Ct);
        finished.Number.Should().Be(1);
        finished.Winners.Should().NotBeEmpty();

        var current = await db.WorldIterations.SingleAsync(i => i.EndedAt == null, Ct);
        current.Number.Should().Be(2);
        current.Winners.Should().BeEmpty();

        (await GetVillagesAsync()).Should().NotBeEmpty();

        var response = await Client.GetAsync("/gameplay/world", Ct);
        response.EnsureSuccessStatusCode();
        var status = await response.Content.ReadFromJsonAsync<WorldStatusDto>(JsonOptions, cancellationToken: Ct);

        status!.Iteration.Should().Be(2);
        status.EndsAt.Should().NotBeNull();
        status.Previous.Should().NotBeNull();
        status.Previous!.Iteration.Should().Be(1);
        status.Previous.Winners.Should().ContainSingle();
        status.BestEver.Should().NotBeNull();
        status.BestEver!.Iteration.Should().Be(1);
        status.BestEver.Score.Should().Be(status.Previous.Winners[0].Score);
    }
}
