using Heat_Production_Optimization.Models;

namespace HeatProductionOptimizationTests;

public class HourSlotTests
{
    [Fact]
    public void Constructor_WithValidHour_StoresDateAndHour()
    {
        var date = new DateOnly(2024, 6, 15);
        var slot = new HourSlot(date, 14);

        Assert.Equal(date, slot.Date);
        Assert.Equal(14, slot.Hour);
    }

    [Fact]
    public void Constructor_WithHourZero_DoesNotThrow()
    {
        var date = new DateOnly(2024, 1, 1);
        var slot = new HourSlot(date, 0);

        Assert.Equal(0, slot.Hour);
    }

    [Fact]
    public void Constructor_WithHour23_DoesNotThrow()
    {
        var date = new DateOnly(2024, 1, 1);
        var slot = new HourSlot(date, 23);

        Assert.Equal(23, slot.Hour);
    }

    [Fact]
    public void Constructor_WithHourNegative_ThrowsArgumentOutOfRangeException()
    {
        var date = new DateOnly(2024, 1, 1);

        Assert.Throws<ArgumentOutOfRangeException>(() => new HourSlot(date, -1));
    }

    [Fact]
    public void Constructor_WithHour24_ThrowsArgumentOutOfRangeException()
    {
        var date = new DateOnly(2024, 1, 1);

        Assert.Throws<ArgumentOutOfRangeException>(() => new HourSlot(date, 24));
    }

    [Fact]
    public void ToDateTime_ReturnsMatchingDateTimeWithCorrectHour()
    {
        var date = new DateOnly(2024, 3, 10);
        var slot = new HourSlot(date, 8);

        var result = slot.ToDateTime();

        Assert.Equal(new DateTime(2024, 3, 10, 8, 0, 0), result);
    }

    [Fact]
    public void ToString_FormatsAsDDMMYYYYHHMM()
    {
        var slot = new HourSlot(new DateOnly(2024, 3, 5), 7);

        Assert.Equal("05.03.2024 07:00", slot.ToString());
    }
}
