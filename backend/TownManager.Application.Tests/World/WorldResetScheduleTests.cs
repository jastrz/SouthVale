using FluentAssertions;
using Xunit;
using TownManager.Application.World;

namespace TownManager.Application.Tests.World;

public class WorldResetScheduleTests
{
    [Theory]
    [InlineData("2026-09-23T14:37:00Z", "2026-09-30T06:00:00Z")] // CEST (UTC+2)
    [InlineData("2026-09-23T03:00:00Z", "2026-09-30T06:00:00Z")]
    [InlineData("2026-09-23T23:30:00Z", "2026-10-01T06:00:00Z")] // late UTC = next local day
    [InlineData("2026-03-25T12:00:00Z", "2026-04-01T06:00:00Z")] // crosses DST start (29 Mar)
    [InlineData("2026-01-15T12:00:00Z", "2026-01-22T07:00:00Z")] // CET (UTC+1)
    public void DueAt_IsResetHourOnTheLocalDayIntervalEnds(string startedAt, string expected)
    {
        WorldResetSchedule.DueAt(DateTime.Parse(startedAt, null, System.Globalization.DateTimeStyles.AdjustToUniversal), 7)
            .Should().Be(DateTime.Parse(expected, null, System.Globalization.DateTimeStyles.AdjustToUniversal));
    }

    [Fact]
    public void DueAt_IsUtc_SoClientsCountDownToTheRightInstant()
    {
        var due = WorldResetSchedule.DueAt(new DateTime(2026, 9, 23, 14, 37, 0, DateTimeKind.Utc), 7);

        due.Kind.Should().Be(DateTimeKind.Utc);
        due.ToString("O").Should().Be("2026-09-30T06:00:00.0000000Z");
    }
}
