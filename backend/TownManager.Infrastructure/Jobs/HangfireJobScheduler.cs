using Hangfire;
using TownManager.Application.Interfaces;

namespace TownManager.Infrastructure.Jobs;

// Note: PerformContext is a special argument type which Hangfire will substitute automatically. You should pass null when enqueuing a job.
// Src: https://libraries.io/nuget/Hangfire.Console 

public class HangfireJobScheduler(IBackgroundJobClient client) : IJobScheduler
{
    public string ScheduleBuildOrderResolution(Guid orderId, TimeSpan buildTime) =>
        client.Schedule<BuildOrderResolutionJob>(
            j => j.ResolveAsync(orderId, null!, CancellationToken.None), buildTime);

    public string ScheduleTrainOrderResolution(Guid orderId, TimeSpan trainingTime) =>
        client.Schedule<TrainOrderResolutionJob>(j => j.ResolveAsync(orderId, null!, CancellationToken.None), trainingTime);

    public string ScheduleMovementResolution(Guid orderId, TimeSpan movementTime) =>
        client.Schedule<TroopMovementResolutionJob>(j => j.ResolveAsync(orderId, CancellationToken.None), movementTime);

    public bool DeleteJob(string jobId) => client.Delete(jobId);
}