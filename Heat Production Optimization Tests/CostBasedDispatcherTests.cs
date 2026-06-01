using Heat_Production_Optimization.Models;
using Heat_Production_Optimization.Optimization;

namespace HeatProductionOptimizationTests;

public class CostBasedDispatcherTests
{
    private static readonly HourSlot AnySlot = new(new DateOnly(2024, 1, 1), 0);

    private static ProductionUnit GasBoiler(string name, double productionCost) =>
        new() { Name = name, UnitType = "GasBoiler", ProductionCost = productionCost, MaxHeat = 100 };

    [Fact]
    public void UnitsAreOrderedByAscendingNetProductionCost()
    {
        var units = new List<ProductionUnit>
        {
            GasBoiler("Expensive", 600),
            GasBoiler("Cheap", 100),
            GasBoiler("Mid", 350)
        };

        var ordered = CostBasedDispatcher.GetDispatchOrderForHour(units, AnySlot, 0);

        Assert.Equal(new[] { "Cheap", "Mid", "Expensive" }, ordered.Select(u => u.Name));
    }

    [Fact]
    public void TieInCostIsResolvedAlphabeticallyByName()
    {
        var units = new List<ProductionUnit>
        {
            GasBoiler("Zeta", 200),
            GasBoiler("Alpha", 200),
            GasBoiler("Mango", 200)
        };

        var ordered = CostBasedDispatcher.GetDispatchOrderForHour(units, AnySlot, 0);

        Assert.Equal(new[] { "Alpha", "Mango", "Zeta" }, ordered.Select(u => u.Name));
    }

    [Fact]
    public void EmptyInputReturnsEmptyList()
    {
        var ordered = CostBasedDispatcher.GetDispatchOrderForHour(
            Enumerable.Empty<ProductionUnit>(), AnySlot, 0);

        Assert.Empty(ordered);
    }

    [Fact]
    public void SingleUnitIsReturnedAsIs()
    {
        var unit = GasBoiler("Solo", 500);

        var ordered = CostBasedDispatcher.GetDispatchOrderForHour(
            new[] { unit }, AnySlot, 50);

        Assert.Single(ordered);
        Assert.Equal("Solo", ordered[0].Name);
    }

    [Fact]
    public void ElectricBoilerRankedHigherWhenPriceIsLow()
    {
        // ElectricBoiler cost = 50 + 1.0 * 10 = 60; GasBoiler cost = 200
        var units = new List<ProductionUnit>
        {
            new() { Name = "Gas", UnitType = "GasBoiler", ProductionCost = 200, MaxHeat = 100 },
            new() { Name = "Electric", UnitType = "ElectricBoiler", ProductionCost = 50, EnergyRate = 1.0, MaxHeat = 100 }
        };

        var ordered = CostBasedDispatcher.GetDispatchOrderForHour(units, AnySlot, electricityPrice: 10);

        Assert.Equal("Electric", ordered[0].Name);
    }
}
