namespace TownManager.Application.World;

public static class WorldResetSchedule
{
    // 08:00 UTC+1
    public const int ResetHourUtc = 7;

    /// <summary>
    /// Reset happens at <see cref="ResetHourUtc"/> UTC on the day the interval ends,
    /// not a full interval after the exact start time.
    /// </summary>
    public static DateTime DueAt(DateTime startedAt, int intervalDays) =>
        startedAt.Date.AddDays(intervalDays).AddHours(ResetHourUtc);
}
