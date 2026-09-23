using FluentAssertions;
using Xunit;
using TownManager.Application.World;

namespace TownManager.Application.Tests.World;

public class WorldResetScheduleTests
{
    [Theory]
    [InlineData("2026-09-23T14:37:00Z", "2026-09-30T07:00:00Z")]
    [InlineData("2026-09-23T03:00:00Z", "2026-09-30T07:00:00Z")]
    [InlineData("2026-09-23T07:00:00Z", "2026-09-30T07:00:00Z")]
    public void DueAt_IsResetHourOnTheDayIntervalEnds(string startedAt, string expected)
    {
        WorldResetSchedule.DueAt(DateTime.Parse(startedAt, null, System.Globalization.DateTimeStyles.AdjustToUniversal), 7)
            .Should().Be(DateTime.Parse(expected, null, System.Globalization.DateTimeStyles.AdjustToUniversal));
    }
}
