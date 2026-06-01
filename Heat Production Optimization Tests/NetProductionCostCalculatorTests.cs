using Heat_Production_Optimization.Models;
using Heat_Production_Optimization.Optimization;

namespace HeatProductionOptimizationTests;

public class NetProductionCostCalculatorTests
{
    private static readonly HourSlot AnySlot = new(new DateOnly(2024, 1, 1), 12);

    private static ProductionUnit BuildUnit(string unitType, double productionCost, double? energyRate = null) =>
        new()
        {
            Name = "TestUnit",
            UnitType = unitType,
            ProductionCost = productionCost,
            EnergyRate = energyRate,
            MaxHeat = 100
        };

    [Fact]
    public void GenericUnit_ReturnsProductionCostDirectly()
    {
        var unit = BuildUnit("GasBoiler", 450);

        var result = NetProductionCostCalculator.ComputeNetProductionCost(unit, AnySlot, 100);

        Assert.Equal(450, result);
    }

    [Fact]
    public void GenericUnit_IgnoresElectricityPrice()
    {
        var unit = BuildUnit("GasBoiler", 300);

        var resultLow = NetProductionCostCalculator.ComputeNetProductionCost(unit, AnySlot, 0);
        var resultHigh = NetProductionCostCalculator.ComputeNetProductionCost(unit, AnySlot, 1000);

        Assert.Equal(resultLow, resultHigh);
    }

    [Fact]
    public void ElectricBoiler_CostIsProductionCostPlusEnergyRateTimesPrice()
    {
        var unit = BuildUnit("ElectricBoiler", 50, energyRate: 2.0);

        var result = NetProductionCostCalculator.ComputeNetProductionCost(unit, AnySlot, 100);

        Assert.Equal(50 + 2.0 * 100, result);
    }

    [Fact]
    public void ElectricBoiler_WithNullEnergyRate_TreatsEnergyRateAsZero()
    {
        var unit = BuildUnit("ElectricBoiler", 50, energyRate: null);

        var result = NetProductionCostCalculator.ComputeNetProductionCost(unit, AnySlot, 200);

        Assert.Equal(50, result);
    }

    [Fact]
    public void CHP_FormulaProducesExpectedCost()
    {
        // cost = (productionCost - energyRate * price + 300) + 0.15 * price
        // cost = (600 - 1.5 * 200 + 300) + 0.15 * 200
        // cost = (600 - 300 + 300) + 30 = 630
        var unit = BuildUnit("CHP", 600, energyRate: 1.5);

        var result = NetProductionCostCalculator.ComputeNetProductionCost(unit, AnySlot, 200);

        Assert.Equal(630, result, precision: 10);
    }

    [Fact]
    public void CHP_ResultNeverFallsBelowNegative50()
    {
        // With a very high electricity price the formula would go very negative —
        // the floor is -50.
        var unit = BuildUnit("CHP", 0, energyRate: 100.0);

        var result = NetProductionCostCalculator.ComputeNetProductionCost(unit, AnySlot, 10_000);

        Assert.Equal(-50, result);
    }

    [Fact]
    public void CHP_WithNullEnergyRate_TreatsEnergyRateAsZero()
    {
        // cost = (500 - 0 * price + 300) + 0.15 * price = 800 + 0.15 * 100 = 815
        var unit = BuildUnit("CHP", 500, energyRate: null);

        var result = NetProductionCostCalculator.ComputeNetProductionCost(unit, AnySlot, 100);

        Assert.Equal(815, result, precision: 10);
    }

    [Fact]
    public void CHP_WithZeroElectricityPrice_UsesBaseFormula()
    {
        // cost = (productionCost - 0 + 300) + 0 = productionCost + 300
        var unit = BuildUnit("CHP", 400, energyRate: 2.0);

        var result = NetProductionCostCalculator.ComputeNetProductionCost(unit, AnySlot, 0);

        Assert.Equal(700, result, precision: 10);
    }
}
