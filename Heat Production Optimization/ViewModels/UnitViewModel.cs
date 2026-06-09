using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Windows.Input;
using Heat_Production_Optimization.Data;
using Heat_Production_Optimization.Services;

namespace Heat_Production_Optimization.ViewModels;

public partial class UnitsViewModel : ViewModelBase, IRecipient<CSVUploadedMessage>
{
    private readonly GraphDataRepository _graphDataRepository = new();

    [ObservableProperty]
    private ObservableCollection<ProductionUnitViewModel> _allUnits = new();

    [ObservableProperty]
    private ProductionUnitViewModel? _selectedUnit;

    public ICommand? ToggleEnableCommand { get; private set; }
    public ICommand? OpenUnitDetailsCommand { get; private set; }

    public UnitsViewModel()
    {
        UpdateUnits();
        WeakReferenceMessenger.Default.Register(this);

        ToggleEnableCommand = new RelayCommand<ProductionUnitViewModel?>(vm =>
        {
            if (vm == null) return;
            WeakReferenceMessenger.Default.Send(new UnitToggledMessage());
        });

        OpenUnitDetailsCommand = new RelayCommand<ProductionUnitViewModel?>(vm =>
        {
            if (vm == null) return;
            var window = new Views.UnitDetails(vm);
            window.Show();
        });
    }

    public void UpdateUnits()
    {
        AllUnits = new ObservableCollection<ProductionUnitViewModel>(
            _graphDataRepository.GetProductionUnits().Select(u => new ProductionUnitViewModel(u)));

        SelectedUnit = AllUnits.Count > 0 ? AllUnits[0] : null;
    }

    public void Receive(CSVUploadedMessage message)
    {
        UpdateUnits();
    }
}
