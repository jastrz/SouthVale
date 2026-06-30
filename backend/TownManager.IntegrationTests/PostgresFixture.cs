using Testcontainers.PostgreSql;

namespace TownManager.IntegrationTests;

public class PostgresFixture : IAsyncLifetime
{
    public PostgreSqlContainer Container { get; } = new PostgreSqlBuilder("postgres:latest")
        .WithDatabase("townmanager_test")
        .Build();

    public async ValueTask InitializeAsync() => await Container.StartAsync();
    public async ValueTask DisposeAsync() => await Container.DisposeAsync();
}

[CollectionDefinition("Integration")]
public class IntegrationCollection : ICollectionFixture<PostgresFixture>;
