using CommunityToolkit.Mvvm.ComponentModel;
using Heat_Production_Optimization.Data;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Heat_Production_Optimization.ViewModels;

public partial class GraphsViewModel : ViewModelBase
{
    [ObservableProperty]
    private string selectedGraph = "Heat Production Over Time";

    private readonly GraphDataRepository _graphDataRepository = new();

    public ObservableCollection<ISeries> Series { get; set; } = new();

    [ObservableProperty]
    private string chartTitle = string.Empty;

    public ObservableCollection<Axis> XAxes { get; set; } = new();
    public ObservableCollection<Axis> YAxes { get; set; } = new();

    private static SolidColorPaint Black() => new SolidColorPaint(SKColors.Black);

    public GraphsViewModel()
    {
        UpdateSeries();
    }

    partial void OnSelectedGraphChanged(string value)
    {
        UpdateSeries();
    }

    public List<string> GraphOptions { get; } = new()
    {
        "Heat Production Over Time",
        "Daily Production Costs",
        "30-Day Efficiency Trends"
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

    private void SetNoDataState(string title)
    {
        ChartTitle = title;
        XAxes.Add(new Axis { Name = "No Data", LabelsPaint = Black(), NamePaint = Black() });
        YAxes.Add(new Axis { Name = "No Data", LabelsPaint = Black(), NamePaint = Black() });
    }
}