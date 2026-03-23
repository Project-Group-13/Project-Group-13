using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace project.ViewModels;

public partial class GraphsViewModel : ViewModelBase
{
    [ObservableProperty]
    private string selectedGraph = "Heat Production Over Time";

    public ObservableCollection<ISeries> Series { get; set; } = new();

    [ObservableProperty]
    private string chartTitle;

    public ObservableCollection<Axis> XAxes { get; set; } = new();
    public ObservableCollection<Axis> YAxes { get; set; } = new();

    private static readonly SolidColorPaint BlackPaint = new SolidColorPaint(SKColors.Black);

    private static SolidColorPaint Black() => new SolidColorPaint(SKColors.Black);

    public GraphsViewModel()
    {
        UpdateSeries();
    }

    partial void OnSelectedGraphChanged(string value)
    {
        UpdateSeries();
    }

    public List<string> GraphOptions { get; } = new() { "Heat Production Over Time", "Daily Production Costs", "30-Day Efficiency Trends" };

    private void UpdateSeries()
    {
        Series.Clear();
        var random = new Random();
        string yAxisName = "";
        string title = "";

        XAxes.Clear();
        YAxes.Clear();

        if (SelectedGraph == "Heat Production Over Time")
        {
            // Filled line chart for heat production over time
            var data = new List<ObservablePoint>();
            for (int i = 0; i < 24; i++) // 24 hours
            {
                data.Add(new ObservablePoint(i, random.Next(0, 50)));
            }
            Series.Add(new LineSeries<ObservablePoint>
            {
                Values = data,
                Name = "Heat Production",
                Fill = new SolidColorPaint(SKColors.LightBlue.WithAlpha(100))
            });
            yAxisName = "Heat (MW)";
            title = "Heat Production Over Time";
            XAxes.Add(new Axis { Labeler = value => $"{(int)value:D2}:00", Name = "Time", MinLimit = 0, MaxLimit = 23, LabelsPaint = Black(), NamePaint = Black() });
            YAxes.Add(new Axis { Name = yAxisName, Labeler = value => $"{value} MW", LabelsPaint = Black(), NamePaint = Black() });
        }
        else if (SelectedGraph == "Daily Production Costs")
        {
            // Filled line chart for daily production costs
            var data = new List<ObservablePoint>();
            for (int i = 1; i <= 7; i++) // Days 1 to 7
            {
                data.Add(new ObservablePoint(i, random.Next(50, 100)));
            }
            Series.Add(new LineSeries<ObservablePoint>
            {
                Values = data,
                Name = "Daily Production Cost",
                Fill = new SolidColorPaint(SKColors.LightCoral.WithAlpha(100))
            });
            yAxisName = "Cost (DKK)";
            title = "Daily Production Costs";
            var labels = new string[8];
            labels[0] = "";
            labels[1] = "Mon";
            labels[2] = "Tue";
            labels[3] = "Wed";
            labels[4] = "Thu";
            labels[5] = "Fri";
            labels[6] = "Sat";
            labels[7] = "Sun";
            XAxes.Add(new Axis { Labels = labels, Name = "Day", MinLimit = 1, MaxLimit = 7, LabelsPaint = Black(), NamePaint = Black() });
            YAxes.Add(new Axis { Name = yAxisName, Labeler = value => $"{value} DKK", LabelsPaint = Black(), NamePaint = Black() });
        }
        else if (SelectedGraph == "30-Day Efficiency Trends")
        {
            // Filled line chart for efficiency trends over 30 days
            var data = new List<ObservablePoint>();
            for (int i = 1; i <= 30; i++) // Days 1 to 30
            {
                data.Add(new ObservablePoint(i, random.Next(70, 100))); // Efficiency 70-100%
            }
            Series.Add(new LineSeries<ObservablePoint>
            {
                Values = data,
                Name = "Efficiency",
                Fill = new SolidColorPaint(SKColors.LightGreen.WithAlpha(100))
            });
            yAxisName = "Efficiency (%)";
            title = "30-Day Efficiency Trends";
            var labels = new string[31];
            labels[0] = "";
            for (int i = 1; i <= 30; i++)
            {
                labels[i] = i.ToString();
            }
            XAxes.Add(new Axis { Labels = labels, Name = "Day", MinLimit = 1, MaxLimit = 30, LabelsPaint = Black(), NamePaint = Black() });
            YAxes.Add(new Axis { Name = yAxisName, Labeler = value => $"{value}%", LabelsPaint = Black(), NamePaint = Black() });
        }

        ChartTitle = title;
    }
}