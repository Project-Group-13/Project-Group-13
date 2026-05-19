using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Heat_Production_Optimization.Models;

public class ProductionUnit : INotifyPropertyChanged
{
    private bool _enabled = false;

    public int UnitId { get; set; }
    public string Name { get; set; } = "";

    public double MaxHeat { get; set; }      // MW
    public double ProductionCost { get; set; } // DkK/MWh
    public double? Co2Emissions { get; set; }  // kg/MWh

    public double? EnergyRate { get; set; }
    public double? MaxElectricity { get; set; }

    public string EnergyType { get; set; } = "";
    public string ImagePath { get; set; } = "";
    public int? GridId { get; set; }

    public string UnitType { get; set; } = "";

    private Bitmap? _image;

    // Whether this unit is enabled for dispatch/selection
    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled == value) return;
            _enabled = value;
            OnPropertyChanged();
        }
    }

    public Bitmap? Image => _image ??= LoadImage();

    private Bitmap? LoadImage()
    {
        var resourcePath = string.IsNullOrWhiteSpace(ImagePath)
            ? "Assets/ProductionUnits/ElectricBoiler.jpg"
            : ImagePath.Trim();

        if (resourcePath.StartsWith("avares://", StringComparison.OrdinalIgnoreCase))
        {
            return new Bitmap(AssetLoader.Open(new Uri(resourcePath)));
        }

        if (resourcePath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
        {
            resourcePath = $"avares://Heat_Production_Optimization/{resourcePath}";
        }

        var uri = new Uri(resourcePath, UriKind.RelativeOrAbsolute);
        var asset = AssetLoader.Open(uri);
        return new Bitmap(asset);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
