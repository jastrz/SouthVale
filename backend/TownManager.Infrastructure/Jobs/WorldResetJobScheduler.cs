using Hangfire;
using Microsoft.Extensions.Hosting;

namespace TownManager.Infrastructure.Jobs;

public class WorldResetJobScheduler(IRecurringJobManager jobs) : IHostedService
{
    private const string Cron = "0 * * * *";

    public Task StartAsync(CancellationToken ct)
    {
        Register(jobs);
        return Task.CompletedTask;
    }

    public static void Register(IRecurringJobManager jobs) =>
        jobs.AddOrUpdate<WorldResetJob>("world-reset",
            j => j.ExecuteAsync(CancellationToken.None), Cron);

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
