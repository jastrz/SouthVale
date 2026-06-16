using Hangfire;
using TownManager.Application.Interfaces;

namespace TownManager.Infrastructure.Jobs;

public class HangfireJobScheduler(IBackgroundJobClient client) : IJobScheduler
{
    public void ScheduleBuildOrderResolution(Guid orderId, TimeSpan buildTime) =>
        client.Schedule<BuildOrderResolutionJob>(
            j => j.ResolveAsync(orderId, CancellationToken.None), buildTime);

    public void ScheduleTrainOrderResolution(Guid orderId, TimeSpan trainingTime) =>
        client.Schedule<TrainOrderResolutionJob>(j => j.ResolveAsync(orderId, CancellationToken.None), trainingTime);
    
    public void ScheduleMovementResolution(Guid orderId, TimeSpan movementTime) =>
        client.Schedule<TroopMovementResolutionJob>(j => j.ResolveAsync(orderId, CancellationToken.None), movementTime);
}