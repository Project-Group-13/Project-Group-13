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

namespace Heat_Production_Optimization;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
            
            var services = new ServiceCollection();
            services.AddSingleton<IFilesService>(x => new FilesService(desktop.MainWindow));

            Services = services.BuildServiceProvider();
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