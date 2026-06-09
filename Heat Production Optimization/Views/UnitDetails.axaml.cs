using Avalonia.Controls;
using Avalonia.Interactivity;
using Heat_Production_Optimization.ViewModels;

namespace Heat_Production_Optimization.Views;

public partial class UnitDetails : Window
{
    public UnitDetails()
    {
        InitializeComponent();
    }

    public UnitDetails(ProductionUnitViewModel vm) : this()
    {
        DataContext = vm;
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
