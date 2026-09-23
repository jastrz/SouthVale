namespace TownManager.Application.World;

public static class WorldResetSchedule
{
    private static readonly TimeZoneInfo ResetTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");

    // 08:00 local (Europe/Warsaw) — tracks CET/CEST instead of a fixed UTC offset.
    public const int ResetHourLocal = 8;

    /// <summary>
    /// Reset happens at <see cref="ResetHourLocal"/> local time on the local day the interval ends,
    /// not a full interval after the exact start time.
    /// </summary>
    public static DateTime DueAt(DateTime startedAt, int intervalDays)
    {
        var localStart = TimeZoneInfo.ConvertTimeFromUtc(startedAt, ResetTimeZone);
        var dueLocal = DateTime.SpecifyKind(
            localStart.Date.AddDays(intervalDays).AddHours(ResetHourLocal),
            DateTimeKind.Unspecified);

        return TimeZoneInfo.ConvertTimeToUtc(dueLocal, ResetTimeZone);
    }
}
