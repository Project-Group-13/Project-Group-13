using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Heat_Production_Optimization.ViewModels;
using Heat_Production_Optimization.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using Heat_Production_Optimization.Services;
using Heat_Production_Optimization.Data;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;


namespace Heat_Production_Optimization;

public partial class App : Application
{
    private DatabaseConnector _dbConn;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        _dbConn = new();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Data.DatabaseInitializer.EnsureCreated();

            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
            
            var services = new ServiceCollection();
            services.AddSingleton<IFilesService>(x => new FilesService(desktop.MainWindow));
            services.AddSingleton<APIService>(x => new APIService(_dbConn));

            Services = services.BuildServiceProvider();

            var apiService = Current?.Services?.GetService<APIService>();

            // Deliberatly calling this method and not waiting
            if(apiService != null) _ = apiService.LoadData();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    public new static App? Current => Application.Current as App;


    public IServiceProvider? Services { get; private set; }

}