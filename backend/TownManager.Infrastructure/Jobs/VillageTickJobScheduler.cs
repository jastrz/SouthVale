using Hangfire;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TownManager.Infrastructure.Jobs;

public class VillageTickJobScheduler(IRecurringJobManager jobs) : IHostedService
{
    private const string Cron = "*/5 * * * *";

    public Task StartAsync(CancellationToken ct)
    {
        Register(jobs, ct);
        return Task.CompletedTask;
    }

    public static void Register(IRecurringJobManager jobs, CancellationToken ct) =>
        jobs.AddOrUpdate<VillageTickJob>("village-tick",
            j => j.ExecuteAsync(ct), Cron);

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
