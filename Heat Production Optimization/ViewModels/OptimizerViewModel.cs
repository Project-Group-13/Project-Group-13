using System;
using System.Linq;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using Heat_Production_Optimization.Models;
using Heat_Production_Optimization.Data;
using Heat_Production_Optimization.Optimization;

namespace Heat_Production_Optimization.ViewModels;

public partial class OptimizerViewModel : ViewModelBase
{
    private readonly Optimizer _optimizer = new();
    private readonly OptimizerHeatDemand _heatDemandRepo = new();
    private readonly GraphDataRepository _unitRepo = new();

    public List<int> Years { get; } = new List<int> { 2025, 2026 };
    public List<int> Months { get; } = Enumerable.Range(1, 12).ToList();
    public List<int> Days { get; } = Enumerable.Range(1, 31).ToList();

    private int _selectedYear = 2026;
    public int SelectedYear
    {
        get => _selectedYear;
        set => SetProperty(ref _selectedYear, value);
    }

    private int _selectedMonth = 1;
    public int SelectedMonth
    {
        get => _selectedMonth;
        set => SetProperty(ref _selectedMonth, value);
    }

    private int _selectedDay = 5;
    public int SelectedDay
    {
        get => _selectedDay;
        set => SetProperty(ref _selectedDay, value);
    }
    public int SelectedHour { get; set; } = 0;
    public List<int> Hours { get; } = Enumerable.Range(0, 24).ToList();

    private string _heatDemandText = "No data yet.";
    public string HeatDemandText
    {
        get => _heatDemandText;
        set => SetProperty(ref _heatDemandText, value);
    }

    private List<OptimizerResult> _results = new();
    public List<OptimizerResult> Results
    {
        get => _results;
        set => SetProperty(ref _results, value);
    }

    public IRelayCommand CallDemandCommand { get; }

    public OptimizerViewModel()
    {
        CallDemandCommand = new RelayCommand(CallDemand);
    }


    public void CallDemand()
    {
        var date = new DateTime(SelectedYear, SelectedMonth, SelectedDay);

        // Heat demand from database
        var heatDemands = _heatDemandRepo.GetHeatDemandForDay(date);
        double heatDemand = heatDemands[SelectedHour];

        HeatDemandText = $"Heat Demand: {heatDemand:F2} MWh";

        // Production units
        var units = _unitRepo.GetProductionUnits();
            

        // HourSlot (no minutes, no seconds)
        var hour = new HourSlot(
            new DateOnly(SelectedYear, SelectedMonth, SelectedDay),
            SelectedHour
        );

        // Run optimizer (Scenario 1)
        Results = _optimizer.Optimize(heatDemand, units, hour);
    }

}