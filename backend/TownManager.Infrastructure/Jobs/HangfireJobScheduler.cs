using Hangfire;
using TownManager.Application.Interfaces;

namespace TownManager.Infrastructure.Jobs;

public class HangfireJobScheduler(IBackgroundJobClient client) : IJobScheduler
{
    public string ScheduleBuildOrderResolution(Guid orderId, TimeSpan buildTime) =>
        client.Schedule<BuildOrderResolutionJob>(
            j => j.ResolveAsync(orderId, CancellationToken.None), buildTime);

    public string ScheduleTrainOrderResolution(Guid orderId, TimeSpan trainingTime) =>
        client.Schedule<TrainOrderResolutionJob>(j => j.ResolveAsync(orderId, CancellationToken.None), trainingTime);

    public string ScheduleMovementResolution(Guid orderId, TimeSpan movementTime) =>
        client.Schedule<TroopMovementResolutionJob>(j => j.ResolveAsync(orderId, CancellationToken.None), movementTime);

    public bool DeleteJob(string jobId) => client.Delete(jobId);
}