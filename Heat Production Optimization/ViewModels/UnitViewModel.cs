using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Heat_Production_Optimization.Data;
using Heat_Production_Optimization.Models;
using Heat_Production_Optimization.Services;
using Microsoft.Extensions.DependencyInjection;

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

    [RelayCommand]
    private async Task UploadUnitsFile(CancellationToken token)
    {
        try
        {
            var filesService = App.Current?.Services?.GetService<IFilesService>();
            if (filesService is null) throw new NullReferenceException("Missing File Service instance.");

            var file = await filesService.UploadFileAsync();
            if (file is null) return;

            if ((await file.GetBasicPropertiesAsync()).Size <= 1024 * 1024 * 1000)
            {
                await using var readStream = await file.OpenReadAsync();
                var data = ProductionUnitParser.Parse(readStream);
                ReplaceProductionUnitsData.ReplaceAll(data);
                WeakReferenceMessenger.Default.Send(new CSVUploadedMessage());
            }
            else
            {
                throw new Exception("File exceeded 1GB limit.");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}