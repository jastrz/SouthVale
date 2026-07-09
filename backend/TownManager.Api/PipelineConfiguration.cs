using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using TownManager.Application.Map.Services;
using TownManager.Api.Hubs;
using TownManager.Infrastructure.Data;
using TownManager.Infrastructure.Identity;
using TownManager.Infrastructure.Persistence;

namespace TownManager.Api;

public static class PipelineConfiguration
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();

        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            db.Database.Migrate();

            if (userManager.Users.Any() == false)
            {
                var seeder = new TestDataSeeder(userManager, db);
                seeder.SeedAsync().GetAwaiter().GetResult();
                Log.Information("Db seeded.");
            }

            var barbarianSeeder = new BarbarianSeeder(db);
            barbarianSeeder.SeedAsync().GetAwaiter().GetResult();
            Log.Information("Barbarian player ready.");

            var llmLogger = scope.ServiceProvider.GetRequiredService<ILogger<LlmPlayerSeeder>>();
            var mapService = scope.ServiceProvider.GetRequiredService<IMapService>();
            var llmSeeder = new LlmPlayerSeeder(userManager, db, mapService, llmLogger);
            llmSeeder.SeedAsync().GetAwaiter().GetResult();
            Log.Information("LLM players ready.");
        }

        app.MapOpenApi();
        app.MapScalarApiReference();

        app.UseCors();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHub<GameHub>("/hubs/game");

        return app;
    }
}
