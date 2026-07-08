using Hangfire;
using TownManager.Application.Players.Services;

namespace TownManager.Infrastructure.Jobs;

[DisableConcurrentExecution(timeoutInSeconds: 300)]
public class LlmPlayerJob(ILlmPlayerService service)
{
    public Task ExecuteAsync(CancellationToken ct) => service.ExecuteAsync(ct);
}
