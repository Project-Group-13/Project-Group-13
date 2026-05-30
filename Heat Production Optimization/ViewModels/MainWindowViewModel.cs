using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Heat_Production_Optimization.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string Greeting { get; } = "Welcome to Avalonia!";

    public UnitsViewModel UnitsViewModel { get; } = new();
    public GraphsViewModel GraphsViewModel { get; }
    public DashBoardViewModel DashBoardViewModel { get; }

    public MainWindowViewModel()
    {
        GraphsViewModel = new GraphsViewModel(UnitsViewModel);
        DashBoardViewModel = new DashBoardViewModel(UnitsViewModel);
    }
}
