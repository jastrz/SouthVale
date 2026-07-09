using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hangfire.PostgreSql;
using Hangfire.PostgreSql.Factories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TownManager.Infrastructure.Persistence;

namespace TownManager.IntegrationTests;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly PostgresFixture _postgres;
    private WebApplicationFactory<Program>? _factory;
    private string _accessToken = string.Empty;

    protected HttpClient Client { get; private set; } = null!;

    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
    };

    protected IntegrationTestBase(PostgresFixture postgres)
    {
        _postgres = postgres;
        Client = null!;
    }

    public async ValueTask InitializeAsync()
    {
        var connectionString = _postgres.Container.GetConnectionString();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:Postgres", connectionString);
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<DbContextOptions<AppDbContext>>();
                    services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
                });
            });

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();

        var hangfireStorage = new PostgreSqlStorage(
            new NpgsqlConnectionFactory(connectionString, new PostgreSqlStorageOptions { PrepareSchemaIfNecessary = true }),
            new PostgreSqlStorageOptions { PrepareSchemaIfNecessary = true });
        using var _ = hangfireStorage.GetConnection();

        Client = _factory.CreateClient();
    }

    public async ValueTask DisposeAsync()
    {
        _factory?.Dispose();
    }

    protected async Task RegisterAsync()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Client.PostAsJsonAsync("/auth/register", new
        {
            email = "test@example.com",
            password = "Test123!",
            username = "TestPlayer",
        }, JsonOptions, ct);

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions, cancellationToken: ct);
        _accessToken = body!.AccessToken;
        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
    }

    protected async Task<List<VillageDto>> GetVillagesAsync()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await Client.GetAsync("/gameplay/me/villages", ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<VillageDto>>(JsonOptions, cancellationToken: ct) ?? [];
    }

    private record AuthResponse(string AccessToken, string Username);
    public record VillageDto(Guid Id, string Name);
}
