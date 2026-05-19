using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Heat_Production_Optimization.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string Greeting { get; } = "Welcome to Avalonia!";

    public GraphsViewModel GraphsViewModel { get; } = new();
    public UnitsViewModel UnitsViewModel { get; } = new();
    public DashBoardViewModel DashBoardViewModel { get; } = new();
}
