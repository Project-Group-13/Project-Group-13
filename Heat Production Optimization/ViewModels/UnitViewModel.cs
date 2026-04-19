using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Heat_Production_Optimization.Data;
using Heat_Production_Optimization.Models;
namespace Heat_Production_Optimization.ViewModels;

public partial class UnitsViewModel : ViewModelBase
{
    private readonly GraphDataRepository _graphDataRepository = new();

    [ObservableProperty]
    private ObservableCollection<ProductionUnit> _allUnits;

    [ObservableProperty]
    private ProductionUnit? _selectedUnit;

    public UnitsViewModel()
    {
        AllUnits = new ObservableCollection<ProductionUnit>(_graphDataRepository.GetProductionUnits());
        SelectedUnit = AllUnits.Count > 0 ? AllUnits[0] : null;
    }
}