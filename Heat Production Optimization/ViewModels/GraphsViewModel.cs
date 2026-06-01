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

public partial class GraphsViewModel : ViewModelBase, IRecipient<CSVUploadedMessage>, IRecipient<UnitToggledMessage>
{
    private const string HeatProductionGraphLabel = "Heat Production Over Time";
    private const string DailyCostGraphLabel = "Daily Production Costs";
    private const string ScheduleGraphLabel = "Heat Demand Schedule (By Date)";
    private const int FallbackMonth = 1;
    private const int FallbackDay = 1;

    [ObservableProperty]
    private string selectedGraph = HeatProductionGraphLabel;

    private readonly UnitsViewModel _unitsViewModel;
    private readonly GraphDataRepository _graphDataRepository = new();
    private readonly Optimizer _optimizer = new();

    private List<DateOnly> _availableDates = new();
    private bool _suppressDateUpdates;
    private bool _suppressRangeUpdates;

    [ObservableProperty]
    private List<int> availableYears = new();

    [ObservableProperty]
    private List<int> availableMonths = new();

    [ObservableProperty]
    private List<int> availableDays = new();

    [ObservableProperty]
    private List<DateOnly> availableDateRange = new();

    [ObservableProperty]
    private int selectedYear = DateTime.Today.Year;

    [ObservableProperty]
    private int selectedMonth = DateTime.Today.Month;

    [ObservableProperty]
    private int selectedDay = DateTime.Today.Day;

    [ObservableProperty]
    private DateOnly selectedRangeStart = DateOnly.FromDateTime(DateTime.Today);

    [ObservableProperty]
    private DateOnly selectedRangeEnd = DateOnly.FromDateTime(DateTime.Today);

    [ObservableProperty]
    private string selectedTimeGrouping = "Days";

    [ObservableProperty]
    private bool isScheduleGraphSelected;

    [ObservableProperty]
    private bool isRangeGraphSelected;

    public ObservableCollection<ISeries> Series { get; set; } = new();

    [ObservableProperty]
    private string chartTitle = string.Empty;

    public ObservableCollection<Axis> XAxes { get; set; } = new();
    public ObservableCollection<Axis> YAxes { get; set; } = new();

    private static SolidColorPaint Black() => new SolidColorPaint(SKColors.Black);

    public GraphsViewModel(UnitsViewModel unitsViewModel)
    {
        _unitsViewModel = unitsViewModel;
        RefreshAvailableDates();
        IsScheduleGraphSelected = SelectedGraph == ScheduleGraphLabel;
        IsRangeGraphSelected = SelectedGraph == HeatProductionGraphLabel || SelectedGraph == DailyCostGraphLabel;
        UpdateSeries();

        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    partial void OnSelectedGraphChanged(string value)
    {
        IsScheduleGraphSelected = value == ScheduleGraphLabel;
        IsRangeGraphSelected = value == HeatProductionGraphLabel || value == DailyCostGraphLabel;
        if (IsScheduleGraphSelected || IsRangeGraphSelected)
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

    partial void OnSelectedRangeStartChanged(DateOnly value)
    {
        if (_suppressRangeUpdates)
        {
            return;
        }

        if (value > SelectedRangeEnd)
        {
            _suppressRangeUpdates = true;
            SelectedRangeEnd = value;
            _suppressRangeUpdates = false;
        }

        if (IsRangeGraphSelected)
        {
            UpdateSeries();
        }
    }

    partial void OnSelectedRangeEndChanged(DateOnly value)
    {
        if (_suppressRangeUpdates)
        {
            return;
        }

        if (value < SelectedRangeStart)
        {
            _suppressRangeUpdates = true;
            SelectedRangeStart = value;
            _suppressRangeUpdates = false;
        }

        if (IsRangeGraphSelected)
        {
            UpdateSeries();
        }
    }

    partial void OnSelectedTimeGroupingChanged(string value)
    {
        if (IsRangeGraphSelected)
        {
            UpdateSeries();
        }
    }

    public List<string> GraphOptions { get; } = new()
    {
        HeatProductionGraphLabel,
        DailyCostGraphLabel,
        "30-Day Efficiency Trends",
        ScheduleGraphLabel
    };

    public List<string> TimeGroupingOptions { get; } = new()
    {
        "Days",
        "Hours"
    };

    private void UpdateSeries()
    {
        Series.Clear();
        XAxes.Clear();
        YAxes.Clear();

        if (SelectedGraph == HeatProductionGraphLabel)
        {
            BuildHeatDemandChart();
        }
        else if (SelectedGraph == DailyCostGraphLabel)
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
        if (!TryGetSelectedRange(out var startDate, out var endDate))
        {
            SetNoDataState("Invalid Date Range");
            return;
        }

        var isHourly = SelectedTimeGrouping == "Hours";
        var sourceData = isHourly
            ? _graphDataRepository.GetHeatDemandSeriesByHour(startDate, endDate)
            : _graphDataRepository.GetHeatDemandSeriesByDay(startDate, endDate);
        if (sourceData.Count > 0)
        {
            var labelFormat = isHourly ? "yyyy-MM-dd HH:00" : "yyyy-MM-dd";
            var labels = sourceData.Select(p => p.Timestamp.ToString(labelFormat)).ToArray();
            var values = sourceData.Select(p => p.Value).ToArray();

            var seriesName = isHourly ? "Heat Production (Hourly)" : "Heat Production (Daily)";

            Series.Add(new ColumnSeries<double>
            {
                Values = values,
                Name = seriesName,
                Fill = new SolidColorPaint(SKColors.LightBlue),
            });

            XAxes.Add(new Axis
            {
                Labels = labels,
                Name = isHourly ? "Hour" : "Day",
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

            var rangeLabel = $"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}";
            ChartTitle = isHourly
                ? $"Heat Production Over Time (Hourly, {rangeLabel})"
                : $"Heat Production Over Time (Daily, {rangeLabel})";
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
        if (!TryGetSelectedRange(out var startDate, out var endDate))
        {
            SetNoDataState("Invalid Date Range");
            return;
        }

        var isHourly = SelectedTimeGrouping == "Hours";
        var costData = isHourly
            ? _graphDataRepository.GetEstimatedCostSeriesByHour(startDate, endDate)
            : _graphDataRepository.GetEstimatedCostSeriesByDay(startDate, endDate);
        if (costData.Count > 0)
        {
            var labelFormat = isHourly ? "yyyy-MM-dd HH:00" : "yyyy-MM-dd";
            var labels = costData.Select(p => p.Timestamp.ToString(labelFormat)).ToArray();
            var values = costData.Select(p => p.Value).ToArray();

            var seriesName = isHourly ? "Production Cost (Hourly)" : "Production Cost (Daily)";

            Series.Add(new ColumnSeries<double>
            {
                Values = values,
                Name = seriesName,
                Fill = new SolidColorPaint(SKColors.LightCoral),
            });

            XAxes.Add(new Axis
            {
                Labels = labels,
                Name = isHourly ? "Hour" : "Day",
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

            var rangeLabel = $"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}";
            ChartTitle = isHourly
                ? $"Production Costs (Hourly, {rangeLabel})"
                : $"Production Costs (Daily, {rangeLabel})";
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

        var heatDemands = _graphDataRepository.GetHeatDemandForDay(date);

        if (heatDemands.Count == 0 || heatDemands.All(value => value <= 0))
        {
            SetNoDataState("No Heat Demand Data For Date");
            return;
        }

        var units = _unitsViewModel.AllUnits
            .Where(vm => vm.Enabled)
            .Select(vm => vm.ProductionUnit)
            .ToList();

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

            double electricityPrice =
                _graphDataRepository.GetElectricityPriceForHour(dateOnly, hour);

            var results = _optimizer.Optimize(
                demand,
                units,
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
        RefreshAvailableDates();
        UpdateSeries();
    }

    private void RefreshAvailableDates()
    {
        _availableDates = _graphDataRepository.GetAvailableHeatDemandDates().ToList();
        if (_availableDates.Count == 0)
        {
            _availableDates.Add(DateOnly.FromDateTime(DateTime.Today));
        }

        AvailableDateRange = _availableDates.ToList();
        _suppressRangeUpdates = true;
        if (!AvailableDateRange.Contains(SelectedRangeStart))
        {
            SelectedRangeStart = AvailableDateRange.First();
        }

        if (!AvailableDateRange.Contains(SelectedRangeEnd))
        {
            SelectedRangeEnd = AvailableDateRange.Last();
        }

        if (SelectedRangeStart > SelectedRangeEnd)
        {
            SelectedRangeEnd = SelectedRangeStart;
        }
        _suppressRangeUpdates = false;

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

    private bool TryGetSelectedRange(out DateOnly startDate, out DateOnly endDate)
    {
        startDate = SelectedRangeStart;
        endDate = SelectedRangeEnd;

        if (startDate == default || endDate == default)
        {
            return false;
        }

        if (startDate > endDate)
        {
            (startDate, endDate) = (endDate, startDate);
        }

        return true;
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

    public void Receive(UnitToggledMessage message)
    {
        UpdateSeries();
    }
}