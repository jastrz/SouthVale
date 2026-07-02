using Hangfire;
using TownManager.Application.Villages.Services;

namespace TownManager.Infrastructure.Jobs;

[DisableConcurrentExecution(timeoutInSeconds: 120)]
public class BarbarianTickJob(IBarbarianTickService service)
{
    public Task ExecuteAsync(CancellationToken ct) => service.ExecuteAsync(ct);
}
