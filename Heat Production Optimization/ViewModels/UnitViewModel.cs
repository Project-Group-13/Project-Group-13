using System.Collections.ObjectModel;
using Avalonia.Automation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Heat_Production_Optimization.Data;
using Heat_Production_Optimization.Models;
using Heat_Production_Optimization.Services;
namespace Heat_Production_Optimization.ViewModels;

public partial class UnitsViewModel : ViewModelBase, IRecipient<CSVUploadedMessage>
{
    private readonly GraphDataRepository _graphDataRepository = new();

    [ObservableProperty]
    private ObservableCollection<ProductionUnit> _allUnits;

    [ObservableProperty]
    private ProductionUnit? _selectedUnit;

    public UnitsViewModel()
    {
        UpdateUnits();
        WeakReferenceMessenger.Default.Register(this);
    }

    public void UpdateUnits()
    {
        AllUnits = new ObservableCollection<ProductionUnit>(_graphDataRepository.GetProductionUnits());
        SelectedUnit = AllUnits.Count > 0 ? AllUnits[0] : null;
    }

    public void Receive(CSVUploadedMessage message)
    {
        UpdateUnits();
    }
}