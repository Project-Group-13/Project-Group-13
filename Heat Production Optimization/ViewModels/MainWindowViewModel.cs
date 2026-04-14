using CommunityToolkit.Mvvm.ComponentModel;

namespace Heat_Production_Optimization.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string Greeting { get; } = "Welcome to Avalonia!";

    public GraphsViewModel GraphsVM { get; } = new();
    public UnitsViewModel UnitsViewModel { get; } = new();
}
