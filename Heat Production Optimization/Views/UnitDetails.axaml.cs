using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Interactivity;
using Heat_Production_Optimization.Models;

namespace Heat_Production_Optimization.Views;

public partial class UnitDetails : Window
{
    public UnitDetails()
    {
        InitializeComponent();
    }

    public UnitDetails(ProductionUnit unit) : this()
    {
        DataContext = unit;
    }

    private static void ApplyDetailToggleAppearance(ToggleButton toggle)
    {
        var isChecked = toggle.IsChecked == true;
        var isPointerOver = toggle.IsPointerOver;

        if (isChecked)
        {
            toggle.Background = new SolidColorBrush(Color.Parse(isPointerOver ? "#15803D" : "#16A34A"));
            toggle.BorderBrush = new SolidColorBrush(Color.Parse(isPointerOver ? "#166534" : "#15803D"));
        }
        else
        {
            toggle.Background = new SolidColorBrush(Color.Parse(isPointerOver ? "#475569" : "#64748B"));
            toggle.BorderBrush = new SolidColorBrush(Color.Parse(isPointerOver ? "#334155" : "#475569"));
        }

        toggle.Foreground = Brushes.White;
    }

    private void DetailToggle_CheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton toggle)
        {
            ApplyDetailToggleAppearance(toggle);
        }
    }

    private void DetailToggle_PointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is ToggleButton toggle)
        {
            ApplyDetailToggleAppearance(toggle);
        }
    }

    private void DetailToggle_PointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is ToggleButton toggle)
        {
            ApplyDetailToggleAppearance(toggle);
        }
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
