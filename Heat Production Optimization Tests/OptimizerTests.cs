using Heat_Production_Optimization.Models;
using Heat_Production_Optimization.Optimization;

namespace HeatProductionOptimizationTests;

public class OptimizerTests
{
    private static readonly HourSlot Slot = new(new DateOnly(2024, 6, 1), 10);
    private static readonly List<MaintenancePeriod> NoMaintenance = new();

    private static ProductionUnit GasBoiler(string name, double maxHeat, double cost) =>
        new() { Name = name, UnitType = "GasBoiler", MaxHeat = maxHeat, ProductionCost = cost };

    [Fact]
    public void SingleUnit_FillsEntireDemandWhenCapacityIsSufficient()
    {
        var optimizer = new Optimizer();
        var units = new List<ProductionUnit> { GasBoiler("GB1", maxHeat: 100, cost: 200) };

        var results = optimizer.Optimize(60, units, Slot, 0, NoMaintenance);

        Assert.Single(results);
        Assert.Equal("GB1", results[0].UnitName);
        Assert.Equal(60, results[0].HeatProduced);
    }

    [Fact]
    public void SingleUnit_CappedAtMaxHeatWhenDemandExceedsCapacity()
    {
        var optimizer = new Optimizer();
        var units = new List<ProductionUnit> { GasBoiler("GB1", maxHeat: 50, cost: 200) };

        var results = optimizer.Optimize(100, units, Slot, 0, NoMaintenance);

        Assert.Single(results);
        Assert.Equal(50, results[0].HeatProduced);
    }

    [Fact]
    public void MultipleUnits_DispatchedInCostOrderUntilDemandSatisfied()
    {
        var optimizer = new Optimizer();
        var units = new List<ProductionUnit>
        {
            GasBoiler("Expensive", maxHeat: 100, cost: 500),
            GasBoiler("Cheap", maxHeat: 40, cost: 100),
            GasBoiler("Mid", maxHeat: 100, cost: 300)
        };

        // Demand = 60: Cheap covers 40, Mid covers remaining 20; Expensive is never used
        var results = optimizer.Optimize(60, units, Slot, 0, NoMaintenance);

        Assert.Equal(2, results.Count);
        Assert.Equal("Cheap", results[0].UnitName);
        Assert.Equal(40, results[0].HeatProduced);
        Assert.Equal("Mid", results[1].UnitName);
        Assert.Equal(20, results[1].HeatProduced);
    }

    [Fact]
    public void DemandAlreadySatisfied_SubsequentUnitsNotDispatched()
    {
        var optimizer = new Optimizer();
        var units = new List<ProductionUnit>
        {
            GasBoiler("First", maxHeat: 100, cost: 100),
            GasBoiler("Second", maxHeat: 100, cost: 200)
        };

        var results = optimizer.Optimize(50, units, Slot, 0, NoMaintenance);

        Assert.Single(results);
        Assert.Equal("First", results[0].UnitName);
    }

    [Fact]
    public void UnitUnderMaintenance_IsSkipped()
    {
        var optimizer = new Optimizer();
        var units = new List<ProductionUnit>
        {
            GasBoiler("GB1", maxHeat: 100, cost: 100),
            GasBoiler("GB2", maxHeat: 100, cost: 200)
        };

        var slotDateTime = Slot.ToDateTime();
        var maintenance = new List<MaintenancePeriod>
        {
            new() { UnitName = "GB1", Start = slotDateTime, DurationHours = 2 }
        };

        var results = optimizer.Optimize(50, units, Slot, 0, maintenance);

        Assert.Single(results);
        Assert.Equal("GB2", results[0].UnitName);
    }

    [Fact]
    public void AllUnitsUnderMaintenance_ReturnsEmpty()
    {
        var optimizer = new Optimizer();
        var units = new List<ProductionUnit>
        {
            GasBoiler("GB1", maxHeat: 100, cost: 100)
        };

        var slotDateTime = Slot.ToDateTime();
        var maintenance = new List<MaintenancePeriod>
        {
            new() { UnitName = "GB1", Start = slotDateTime, DurationHours = 4 }
        };

        var results = optimizer.Optimize(50, units, Slot, 0, maintenance);

        Assert.Empty(results);
    }

    [Fact]
    public void NoUnits_ReturnsEmpty()
    {
        var optimizer = new Optimizer();

        var results = optimizer.Optimize(50, new List<ProductionUnit>(), Slot, 0, NoMaintenance);

        Assert.Empty(results);
    }

    [Fact]
    public void ZeroDemand_ReturnsEmpty()
    {
        var optimizer = new Optimizer();
        var units = new List<ProductionUnit> { GasBoiler("GB1", maxHeat: 100, cost: 100) };

        var results = optimizer.Optimize(0, units, Slot, 0, NoMaintenance);

        Assert.Empty(results);
    }

    [Fact]
    public void MaintenanceExpired_UnitIsIncluded()
    {
        var optimizer = new Optimizer();
        var units = new List<ProductionUnit>
        {
            GasBoiler("GB1", maxHeat: 100, cost: 100)
        };

        var slotDateTime = Slot.ToDateTime();
        // maintenance ended 2 hours before the slot
        var maintenance = new List<MaintenancePeriod>
        {
            new() { UnitName = "GB1", Start = slotDateTime.AddHours(-4), DurationHours = 2 }
        };

        var results = optimizer.Optimize(50, units, Slot, 0, maintenance);

        Assert.Single(results);
        Assert.Equal("GB1", results[0].UnitName);
    }
}
