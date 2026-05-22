using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Heat_Production_Optimization.Data;
using Heat_Production_Optimization.Services;
using Heat_Production_Optimization.Models;
using Heat_Production_Optimization.Optimization;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Heat_Production_Optimization.ViewModels;

public partial class GraphsViewModel : ViewModelBase, IRecipient<CSVUploadedMessage>
{
    private const string ScheduleGraphLabel = "Heat Demand Schedule (By Date)";
    private const int FallbackMonth = 1;
    private const int FallbackDay = 1;

    [ObservableProperty]
    private string selectedGraph = "Heat Production Over Time";

    private readonly GraphDataRepository _graphDataRepository = new();
    private readonly OptimizerHeatDemand _heatDemandRepo = new();
    private readonly Optimizer _optimizer = new();

    private List<DateOnly> _availableDates = new();
    private bool _suppressDateUpdates;

    [ObservableProperty]
    private List<int> availableYears = new();

    [ObservableProperty]
    private List<int> availableMonths = new();

    [ObservableProperty]
    private List<int> availableDays = new();

    [ObservableProperty]
    private int selectedYear = DateTime.Today.Year;

    [ObservableProperty]
    private int selectedMonth = DateTime.Today.Month;

    [ObservableProperty]
    private int selectedDay = DateTime.Today.Day;

    [ObservableProperty]
    private bool isScheduleGraphSelected;

    public ObservableCollection<ISeries> Series { get; set; } = new();

    [ObservableProperty]
    private string chartTitle = string.Empty;

    public ObservableCollection<Axis> XAxes { get; set; } = new();
    public ObservableCollection<Axis> YAxes { get; set; } = new();

    private static SolidColorPaint Black() => new SolidColorPaint(SKColors.Black);

    public GraphsViewModel()
    {
        RefreshAvailableDates();
        IsScheduleGraphSelected = SelectedGraph == ScheduleGraphLabel;
        UpdateSeries();

        WeakReferenceMessenger.Default.Register(this);
    }

    partial void OnSelectedGraphChanged(string value)
    {
        IsScheduleGraphSelected = value == ScheduleGraphLabel;
        if (IsScheduleGraphSelected)
        {
            RefreshAvailableDates();
        }
        UpdateSeries();
    }

    partial void OnSelectedYearChanged(int value)
    {
        if (_suppressDateUpdates)
        {
            return;
        }

        _suppressDateUpdates = true;
        SyncMonthAndDayForYear(value);
        _suppressDateUpdates = false;

        if (IsScheduleGraphSelected)
        {
            UpdateSeries();
        }
    }

    partial void OnSelectedMonthChanged(int value)
    {
        if (_suppressDateUpdates)
        {
            return;
        }

        _suppressDateUpdates = true;
        SyncDaysForMonth(SelectedYear, value);
        _suppressDateUpdates = false;

        if (IsScheduleGraphSelected)
        {
            UpdateSeries();
        }
    }

    partial void OnSelectedDayChanged(int value)
    {
        if (_suppressDateUpdates)
        {
            return;
        }

        if (IsScheduleGraphSelected)
        {
            UpdateSeries();
        }
    }

    public List<string> GraphOptions { get; } = new()
    {
        "Heat Production Over Time",
        "Daily Production Costs",
        "30-Day Efficiency Trends",
        ScheduleGraphLabel
    };

    private void UpdateSeries()
    {
        Series.Clear();
        XAxes.Clear();
        YAxes.Clear();

        if (SelectedGraph == "Heat Production Over Time")
        {
            BuildHeatDemandChart();
        }
        else if (SelectedGraph == "Daily Production Costs")
        {
            BuildDailyCostChart();
        }
        else if (SelectedGraph == "30-Day Efficiency Trends")
        {
            BuildEfficiencyChart();
        }
        else if (SelectedGraph == ScheduleGraphLabel)
        {
            BuildDailyScheduleChart();
        }
    }

    private void BuildHeatDemandChart()
    {
        var sourceData = _graphDataRepository.GetHeatDemandSeries();
        if (sourceData.Count > 0)
        {
            var labels = sourceData.Select(p => p.Timestamp.ToString("MM-dd HH:mm")).ToArray();
            var values = sourceData.Select(p => p.Value).ToArray();

            Series.Add(new ColumnSeries<double>
            {
                Values = values,
                Name = "Heat Production",
                Fill = new SolidColorPaint(SKColors.LightBlue),
            });

            XAxes.Add(new Axis
            {
                Labels = labels,
                Name = "Time",
                LabelsPaint = Black(),
                NamePaint = Black()
            });

            YAxes.Add(new Axis
            {
                Name = "Heat (MW)",
                Labeler = value => $"{value:0.##} MW",
                LabelsPaint = Black(),
                NamePaint = Black()
            });

            ChartTitle = "Heat Production Over Time";
            return;
        }

        // Fallback to unit capacities when no time-series source rows exist.
        var unitHeat = _graphDataRepository.GetUnitMaxHeatSeries();
        if (unitHeat.Count == 0)
        {
            SetNoDataState("No Heat Data Found");
            return;
        }

        var fallbackLabels = unitHeat.Select(p => p.UnitName).ToArray();
        var fallbackValues = unitHeat.Select(p => p.Value).ToArray();

        Series.Add(new ColumnSeries<double>
        {
            Values = fallbackValues,
            Name = "Unit Max Heat",
            Fill = new SolidColorPaint(SKColors.LightBlue),
        });

        XAxes.Add(new Axis
        {
            Labels = fallbackLabels,
            Name = "Unit",
            LabelsPaint = Black(),
            NamePaint = Black()
        });

        YAxes.Add(new Axis
        {
            Name = "Heat (MW)",
            Labeler = value => $"{value:0.##} MW",
            LabelsPaint = Black(),
            NamePaint = Black()
        });

        ChartTitle = "Heat Production Over Time (Unit Fallback)";
    }

    private void BuildDailyCostChart()
    {
        var costData = _graphDataRepository.GetEstimatedDailyCostSeries();
        if (costData.Count > 0)
        {
            var labels = costData.Select(p => p.Timestamp.ToString("MM-dd")).ToArray();
            var values = costData.Select(p => p.Value).ToArray();

            Series.Add(new ColumnSeries<double>
            {
                Values = values,
                Name = "Daily Production Cost",
                Fill = new SolidColorPaint(SKColors.LightCoral),
            });

            XAxes.Add(new Axis
            {
                Labels = labels,
                Name = "Day",
                LabelsPaint = Black(),
                NamePaint = Black()
            });

            YAxes.Add(new Axis
            {
                Name = "Cost (DKK)",
                Labeler = value => $"{value:0.##} DKK",
                LabelsPaint = Black(),
                NamePaint = Black()
            });

            ChartTitle = "Daily Production Costs";
            return;
        }

        // Fallback to unit production cost when no daily source rows exist.
        var unitCost = _graphDataRepository.GetUnitCostSeries();
        if (unitCost.Count == 0)
        {
            SetNoDataState("No Production Cost Data Found");
            return;
        }

        var fallbackLabels = unitCost.Select(p => p.UnitName).ToArray();
        var fallbackValues = unitCost.Select(p => p.Value).ToArray();

        Series.Add(new ColumnSeries<double>
        {
            Values = fallbackValues,
            Name = "Production Cost",
            Fill = new SolidColorPaint(SKColors.LightCoral),
        });

        XAxes.Add(new Axis
        {
            Labels = fallbackLabels,
            Name = "Unit",
            LabelsPaint = Black(),
            NamePaint = Black()
        });

        YAxes.Add(new Axis
        {
            Name = "Cost (DKK)",
            Labeler = value => $"{value:0.##} DKK",
            LabelsPaint = Black(),
            NamePaint = Black()
        });

        ChartTitle = "Daily Production Costs (Unit Fallback)";
    }

    private void BuildEfficiencyChart()
    {
        var efficiencyData = _graphDataRepository.GetUnitEnergyRateSeries();
        if (efficiencyData.Count == 0)
        {
            SetNoDataState("No Efficiency Data Found");
            return;
        }

        var labels = efficiencyData.Select(p => p.UnitName).ToArray();
        var values = efficiencyData.Select(p => p.Value).ToArray();

        Series.Add(new ColumnSeries<double>
        {
            Values = values,
            Name = "Efficiency",
            Fill = new SolidColorPaint(SKColors.LightGreen),
        });

        XAxes.Add(new Axis
        {
            Labels = labels,
            Name = "Unit",
            LabelsPaint = Black(),
            NamePaint = Black()
        });

        YAxes.Add(new Axis
        {
            Name = "Efficiency",
            Labeler = value => $"{value:0.##}",
            LabelsPaint = Black(),
            NamePaint = Black()
        });

        ChartTitle = "30-Day Efficiency Trends";
    }

    private void BuildDailyScheduleChart()
    {
        DateTime date;
        try
        {
            date = new DateTime(SelectedYear, SelectedMonth, SelectedDay);
        }
        catch
        {
            SetNoDataState("Invalid Date");
            return;
        }

        var heatDemands = _heatDemandRepo.GetHeatDemandForDay(date);
        if (heatDemands.Count == 0 || heatDemands.All(value => value <= 0))
        {
            SetNoDataState("No Heat Demand Data For Date");
            return;
        }

        var units = _graphDataRepository.GetProductionUnits();
        if (units.Count == 0)
        {
            SetNoDataState("No Production Units Found");
            return;
        }

        var unitNames = units.Select(unit => unit.Name).Distinct().ToList();
        var unitSeries = unitNames.ToDictionary(name => name, _ => new double[24]);
        var dateOnly = new DateOnly(SelectedYear, SelectedMonth, SelectedDay);

        for (var hour = 0; hour < 24; hour++) 
        {
            var demand = hour < heatDemands.Count ? heatDemands[hour] : 0;
            if (demand <= 0)
            {
                continue;
            }

            double electricityPrice = 0;

            var results = _optimizer.Optimize(
                demand, 
                units.ToList(),
                new HourSlot(dateOnly, hour), 
                electricityPrice,
                MaintenanceSchedule.GetSchedule()

            );

            foreach (var result in results)
            {
                if (unitSeries.TryGetValue(result.UnitName, out var values))
                {
                    values[hour] = result.HeatProduced;
                }
            }
        }

        var palette = new[]
        {
            SKColors.SteelBlue,
            SKColors.MediumSeaGreen,
            SKColors.SandyBrown,
            SKColors.MediumPurple,
            SKColors.IndianRed,
            SKColors.CadetBlue,
            SKColors.Goldenrod,
            SKColors.LightSlateGray
        };

        for (var i = 0; i < unitNames.Count; i++)
        {
            var unitName = unitNames[i];
            Series.Add(new StackedColumnSeries<double>
            {
                Values = unitSeries[unitName],
                Name = unitName,
                Fill = new SolidColorPaint(palette[i % palette.Length])
            });
        }

        Series.Add(new LineSeries<double>
        {
            Values = heatDemands.Take(24).ToArray(),
            Name = "Heat Demand",
            Stroke = new SolidColorPaint(SKColors.DarkOrange, 3),
            Fill = null,
            GeometryFill = new SolidColorPaint(SKColors.White),
            GeometryStroke = new SolidColorPaint(SKColors.DarkOrange, 3)
        });

        XAxes.Add(new Axis
        {
            Labels = Enumerable.Range(0, 24).Select(hour => $"{hour:00}:00").ToArray(),
            Name = "Hour",
            LabelsPaint = Black(),
            NamePaint = Black()
        });

        YAxes.Add(new Axis
        {
            Name = "Heat (MW)",
            Labeler = value => $"{value:0.##} MW",
            LabelsPaint = Black(),
            NamePaint = Black()
        });

        ChartTitle = $"Heat Demand Schedule ({date:yyyy-MM-dd})";
    }

    private void SetNoDataState(string title)
    {
        ChartTitle = title;
        XAxes.Add(new Axis { Name = "No Data", LabelsPaint = Black(), NamePaint = Black() });
        YAxes.Add(new Axis { Name = "No Data", LabelsPaint = Black(), NamePaint = Black() });
    }

    public void Receive(CSVUploadedMessage message)
    {
        UpdateSeries();
    }
    
    private void RefreshAvailableDates()
    {
        _availableDates = _graphDataRepository.GetAvailableHeatDemandDates().ToList();
        if (_availableDates.Count == 0)
        {
            _availableDates.Add(DateOnly.FromDateTime(DateTime.Today));
        }

        AvailableYears = _availableDates
            .Select(date => date.Year)
            .Distinct()
            .OrderBy(year => year)
            .ToList();

        if (AvailableYears.Count == 0)
        {
            AvailableYears = new List<int> { DateTime.Today.Year };
        }

        _suppressDateUpdates = true;
        if (!AvailableYears.Contains(SelectedYear))
        {
            SelectedYear = AvailableYears.Last();
        }

        SyncMonthAndDayForYear(SelectedYear);
        _suppressDateUpdates = false;
    }

    private void SyncMonthAndDayForYear(int year)
    {
        var months = _availableDates
            .Where(date => date.Year == year)
            .Select(date => date.Month)
            .Distinct()
            .OrderBy(month => month)
            .ToList();

        if (months.Count == 0)
        {
            months.Add(FallbackMonth);
        }

        AvailableMonths = months;
        if (!AvailableMonths.Contains(SelectedMonth))
        {
            SelectedMonth = AvailableMonths.Last();
        }

        SyncDaysForMonth(year, SelectedMonth);
    }

    private void SyncDaysForMonth(int year, int month)
    {
        var days = _availableDates
            .Where(date => date.Year == year && date.Month == month)
            .Select(date => date.Day)
            .Distinct()
            .OrderBy(day => day)
            .ToList();

        if (days.Count == 0)
        {
            days.Add(FallbackDay);
        }

        AvailableDays = days;
        if (!AvailableDays.Contains(SelectedDay))
        {
            SelectedDay = AvailableDays.Last();
        }
    }
}