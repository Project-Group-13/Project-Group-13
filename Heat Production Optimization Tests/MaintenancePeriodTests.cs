using Heat_Production_Optimization.Models;

namespace HeatProductionOptimizationTests;

public class MaintenancePeriodTests
{
    private static MaintenancePeriod BuildPeriod(DateTime start, int durationHours) =>
        new() { UnitName = "GB1", Start = start, DurationHours = durationHours };

    [Fact]
    public void End_IsStartPlusDurationHours()
    {
        var start = new DateTime(2024, 6, 1, 8, 0, 0);
        var period = BuildPeriod(start, 4);

        Assert.Equal(new DateTime(2024, 6, 1, 12, 0, 0), period.End);
    }

    [Fact]
    public void IsUnderMaintenance_AtStartHour_ReturnsTrue()
    {
        var start = new DateTime(2024, 6, 1, 8, 0, 0);
        var period = BuildPeriod(start, 4);

        Assert.True(period.IsUnderMaintenance(start));
    }

    [Fact]
    public void IsUnderMaintenance_DuringWindow_ReturnsTrue()
    {
        var start = new DateTime(2024, 6, 1, 8, 0, 0);
        var period = BuildPeriod(start, 4);

        Assert.True(period.IsUnderMaintenance(new DateTime(2024, 6, 1, 10, 0, 0)));
    }

    [Fact]
    public void IsUnderMaintenance_AtEndHour_ReturnsFalse()
    {
        var start = new DateTime(2024, 6, 1, 8, 0, 0);
        var period = BuildPeriod(start, 4);

        Assert.False(period.IsUnderMaintenance(period.End));
    }

    [Fact]
    public void IsUnderMaintenance_BeforeStart_ReturnsFalse()
    {
        var start = new DateTime(2024, 6, 1, 8, 0, 0);
        var period = BuildPeriod(start, 4);

        Assert.False(period.IsUnderMaintenance(new DateTime(2024, 6, 1, 7, 0, 0)));
    }

    [Fact]
    public void IsUnderMaintenance_AfterEnd_ReturnsFalse()
    {
        var start = new DateTime(2024, 6, 1, 8, 0, 0);
        var period = BuildPeriod(start, 4);

        Assert.False(period.IsUnderMaintenance(new DateTime(2024, 6, 1, 13, 0, 0)));
    }
}
