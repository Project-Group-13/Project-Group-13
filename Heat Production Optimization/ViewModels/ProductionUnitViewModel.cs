using System;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using Heat_Production_Optimization.Models;

namespace Heat_Production_Optimization.ViewModels;

public partial class ProductionUnitViewModel : ViewModelBase
{
    private readonly ProductionUnit _productionUnit;
    private Bitmap? _image;

    [ObservableProperty]
    private bool _enabled = true;

    public ProductionUnitViewModel(ProductionUnit model)
    {
        _productionUnit = model;
    }

    public ProductionUnit ProductionUnit => _productionUnit;

    public int UnitId => _productionUnit.UnitId;
    public string Name => _productionUnit.Name;
    public double MaxHeat => _productionUnit.MaxHeat;
    public double ProductionCost => _productionUnit.ProductionCost;
    public double? Co2Emissions => _productionUnit.Co2Emissions;
    public double? EnergyRate => _productionUnit.EnergyRate;
    public double? MaxElectricity => _productionUnit.MaxElectricity;
    public string EnergyType => _productionUnit.EnergyType;
    public string UnitType => _productionUnit.UnitType;
    public int? GridId => _productionUnit.GridId;

    public Bitmap? Image => _image ??= LoadImage();

    private Bitmap? LoadImage()
    {
        var resourcePath = string.IsNullOrWhiteSpace(_productionUnit.ImagePath)
            ? "Assets/ProductionUnits/ElectricBoiler.jpg"
            : _productionUnit.ImagePath.Trim();

        if (resourcePath.StartsWith("avares://", StringComparison.OrdinalIgnoreCase))
            return new Bitmap(AssetLoader.Open(new Uri(resourcePath)));

        if (resourcePath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            resourcePath = $"avares://Heat_Production_Optimization/{resourcePath}";

        return new Bitmap(AssetLoader.Open(new Uri(resourcePath, UriKind.RelativeOrAbsolute)));
    }
}
