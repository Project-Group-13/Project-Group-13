using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Heat_Production_Optimization.Services;
using Heat_Production_Optimization.Models;
using Microsoft.Extensions.DependencyInjection;
using Heat_Production_Optimization.Data;
using CommunityToolkit.Mvvm.Messaging;
using System.IO;

namespace Heat_Production_Optimization.ViewModels;

public partial class DashBoardViewModel : ViewModelBase
{
    [ObservableProperty] private string? _fileText;

    public DashBoardViewModel()
    {
    }

    [RelayCommand]
    private async Task UploadSourceDataFile(CancellationToken token)
        => await UploadFile(CSVParser.Parse<SourceData, SourceDataMap>, ReplaceSourceData.ReplaceAll, token);

    [RelayCommand]
    private async Task UploadUnitsFile(CancellationToken token)
      => await UploadFile(CSVParser.Parse<ProductionUnit, ProductionUnitMap>, ReplaceProductionUnitsData.ReplaceAll, token);

    private async Task UploadFile<T>(Func<Stream, T> parser, Action<T> replacer, CancellationToken token)
    {
        try
        {
            var filesService = App.Current?.Services?.GetService<IFilesService>();
            if (filesService is null) throw new NullReferenceException("Missing File Service instance.");

            var file = await filesService.UploadFileAsync();
            if (file is null) return;

            // Limit the file to 1GB.
            if ((await file.GetBasicPropertiesAsync()).Size <= 1024 * 1024 * 1000)
            {
                await using var readStream = await file.OpenReadAsync();
                var data = parser(readStream);
                replacer(data);
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