using Avalonia.Headless.XUnit;
using Heat_Production_Optimization.Models;
using Heat_Production_Optimization.ViewModels;

namespace HeatProductionOptimizationTests;

public class ProductionUnitViewModelTests
{
    private static ProductionUnit BuildUnit() => new()
    {
        UnitId = 7,
        Name = "GB1",
        MaxHeat = 80,
        ProductionCost = 400,
        Co2Emissions = 215,
        EnergyRate = 1.5,
        MaxElectricity = 20,
        EnergyType = "Gas",
        UnitType = "CHP",
        GridId = 3
    };

    [AvaloniaFact]
    public void Enabled_DefaultsToTrue()
    {
        var vm = new ProductionUnitViewModel(BuildUnit());

        Assert.True(vm.Enabled);
    }

    [AvaloniaFact]
    public void Enabled_CanBeToggledToFalse()
    {
        var vm = new ProductionUnitViewModel(BuildUnit());

        vm.Enabled = false;

        Assert.False(vm.Enabled);
    }

    [AvaloniaFact]
    public void Enabled_CanBeToggledBackToTrue()
    {
        var vm = new ProductionUnitViewModel(BuildUnit());
        vm.Enabled = false;

        vm.Enabled = true;

        Assert.True(vm.Enabled);
    }

    [AvaloniaFact]
    public void ProductionUnit_ReturnsUnderlyingModel()
    {
        var model = BuildUnit();
        var vm = new ProductionUnitViewModel(model);

        Assert.Same(model, vm.ProductionUnit);
    }

    [AvaloniaFact]
    public void UnitId_ExposesUnderlyingModelValue()
    {
        var vm = new ProductionUnitViewModel(BuildUnit());

        Assert.Equal(7, vm.UnitId);
    }

    [AvaloniaFact]
    public void Name_ExposesUnderlyingModelValue()
    {
        var vm = new ProductionUnitViewModel(BuildUnit());

        Assert.Equal("GB1", vm.Name);
    }

    [AvaloniaFact]
    public void MaxHeat_ExposesUnderlyingModelValue()
    {
        var vm = new ProductionUnitViewModel(BuildUnit());

        Assert.Equal(80, vm.MaxHeat);
    }

    [AvaloniaFact]
    public void ProductionCost_ExposesUnderlyingModelValue()
    {
        var vm = new ProductionUnitViewModel(BuildUnit());

        Assert.Equal(400, vm.ProductionCost);
    }

    [AvaloniaFact]
    public void Co2Emissions_ExposesUnderlyingModelValue()
    {
        var vm = new ProductionUnitViewModel(BuildUnit());

        Assert.Equal(215, vm.Co2Emissions);
    }

    [AvaloniaFact]
    public void EnergyRate_ExposesUnderlyingModelValue()
    {
        var vm = new ProductionUnitViewModel(BuildUnit());

        Assert.Equal(1.5, vm.EnergyRate);
    }

    [AvaloniaFact]
    public void MaxElectricity_ExposesUnderlyingModelValue()
    {
        var vm = new ProductionUnitViewModel(BuildUnit());

        Assert.Equal(20, vm.MaxElectricity);
    }

    [AvaloniaFact]
    public void EnergyType_ExposesUnderlyingModelValue()
    {
        var vm = new ProductionUnitViewModel(BuildUnit());

        Assert.Equal("Gas", vm.EnergyType);
    }

    [AvaloniaFact]
    public void UnitType_ExposesUnderlyingModelValue()
    {
        var vm = new ProductionUnitViewModel(BuildUnit());

        Assert.Equal("CHP", vm.UnitType);
    }

    [AvaloniaFact]
    public void GridId_ExposesUnderlyingModelValue()
    {
        var vm = new ProductionUnitViewModel(BuildUnit());

        Assert.Equal(3, vm.GridId);
    }

    [AvaloniaFact]
    public void NullableProperties_ReturnNullWhenNotSet()
    {
        var model = new ProductionUnit { Name = "Empty" };
        var vm = new ProductionUnitViewModel(model);

        Assert.Null(vm.Co2Emissions);
        Assert.Null(vm.EnergyRate);
        Assert.Null(vm.MaxElectricity);
        Assert.Null(vm.GridId);
    }
}
