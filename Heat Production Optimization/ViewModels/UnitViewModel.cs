using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
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

    public ICommand? SelectUnitCommand { get; private set; }
    public ICommand? ToggleEnableCommand { get; private set; }
    public ICommand? OpenUnitDetailsCommand { get; private set; }

    public UnitsViewModel()
    {
        AllUnits = new ObservableCollection<ProductionUnit>(_graphDataRepository.GetProductionUnits());
        SelectedUnit = AllUnits.Count > 0 ? AllUnits[0] : null;

        // Ensure all units start disabled per request
        foreach (var u in AllUnits)
        {
            u.Enabled = false;
        }

        SelectUnitCommand = new RelayCommand<ProductionUnit?>(u =>
        {
            if (u != null)
                SelectedUnit = u;
        });

        ToggleEnableCommand = new RelayCommand<ProductionUnit?>(u =>
        {
            if (u != null)
                u.Enabled = !u.Enabled;
        });

        OpenUnitDetailsCommand = new RelayCommand<ProductionUnit?>(u =>
        {
            if (u == null) return;
            // Open a details window for the selected unit
            var wnd = new Heat_Production_Optimization.Views.UnitDetails(u);
            wnd.Show();
        });
    }
}