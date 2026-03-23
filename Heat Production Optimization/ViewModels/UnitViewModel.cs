using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using project.Models; 
namespace project.ViewModels;

public partial class UnitsViewModel : ViewModelBase
{

    

    [ObservableProperty]
    private ObservableCollection<ProductionUnit> _allUnits;

    [ObservableProperty]
    private ProductionUnit? _selectedUnit;

    public UnitsViewModel()
    {

        AllUnits = new ObservableCollection<ProductionUnit>
        {
            new ProductionUnit { Name = "Gas Boiler 1", MaxHeat = 5.0, ProductionCost = 500, Co2Emissions = 215 },
            new ProductionUnit { Name = "Gas Boiler 2", MaxHeat = 5.0, ProductionCost = 500, Co2Emissions = 215 },
            new ProductionUnit { Name = "Oil Boiler", MaxHeat = 4.0, ProductionCost = 700, Co2Emissions = 265 }
        };
    }
}