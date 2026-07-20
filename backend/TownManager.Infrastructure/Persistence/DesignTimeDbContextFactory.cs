using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TownManager.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var apiDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "TownManager.Api");
        if (!Directory.Exists(apiDir))
            apiDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "TownManager.Api"); // running from Infra dir
        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiDir)
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("Postgres"));

        return new AppDbContext(optionsBuilder.Options);
    }
}
