using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Heat_Production_Optimization.Data;
using Heat_Production_Optimization.Models;
using Heat_Production_Optimization.Optimization;
using Heat_Production_Optimization.Services;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace Heat_Production_Optimization.ViewModels;

public partial class DashBoardViewModel : ViewModelBase, IRecipient<CSVUploadedMessage>
{
    private const int FallbackMonth = 1;
    private const int FallbackDay = 1;

    private readonly GraphDataRepository _graphDataRepository = new();
    private readonly Optimizer _optimizer = new();

    private List<DateOnly> _availableDates = new();
    private bool _suppressDateUpdates;

    [ObservableProperty] private List<int> availableYears = new();
    [ObservableProperty] private List<int> availableMonths = new();
    [ObservableProperty] private List<int> availableDays = new();

    [ObservableProperty] private int selectedYear = DateTime.Today.Year;
    [ObservableProperty] private int selectedMonth = DateTime.Today.Month;
    [ObservableProperty] private int selectedDay = DateTime.Today.Day;
    [ObservableProperty] private int selectedHour;

    [ObservableProperty] private string statusMessage = "No data loaded.";
    [ObservableProperty] private double heatDemand;
    [ObservableProperty] private double totalHeatProduced;
    [ObservableProperty] private double totalNetProductionCost;
    [ObservableProperty] private double totalCo2Emissions;
    [ObservableProperty] private double netCostPerMwh;
    [ObservableProperty] private int activeUnitsCount;
    [ObservableProperty] private string activeUnitsSummary = "No active units.";
    [ObservableProperty] private bool showEmptyState = true;

    [ObservableProperty] private ObservableCollection<DashboardUnitResult> activeUnits = new();

    public List<int> Hours { get; } = Enumerable.Range(0, 24).ToList();
    public IRelayCommand RefreshDashboardCommand { get; }

    public DashBoardViewModel()
    {
        RefreshDashboardCommand = new RelayCommand(RefreshDashboard);
        RefreshAvailableDates();
        RefreshDashboard();

        WeakReferenceMessenger.Default.Register(this);
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
        RefreshDashboard();
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
        RefreshDashboard();
    }

    partial void OnSelectedDayChanged(int value)
    {
        if (_suppressDateUpdates)
        {
            return;
        }

        RefreshDashboard();
    }

    partial void OnSelectedHourChanged(int value)
    {
        RefreshDashboard();
    }

    [RelayCommand]
    private async Task UploadSourceDataFile(CancellationToken token)
    {
        await UploadFile(CSVParser.Parse<SourceData, SourceDataMap>, ReplaceSourceData.ReplaceAll, token);

        var apiService = App.Current?.Services?.GetService<APIService>();

        if(apiService != null) _ = apiService.LoadData();
    }

    [RelayCommand]
    private async Task UploadUnitsFile(CancellationToken token)
      => await UploadFile(CSVParser.Parse<ProductionUnit, ProductionUnitMap>, ReplaceProductionUnitsData.ReplaceAll, token);

    private async Task UploadFile<T>(Func<Stream, T> parser, Action<T> replacer, CancellationToken token)
    {
        try
        {
            var filesService = App.Current?.Services?.GetService<IFilesService>();
            if (filesService is null) throw new NullReferenceException("Missing File Service instance.");

            var file = await filesService.UploadFileAsync();
            if (file is null) return;

            // Limit the file to 1GB.
            if ((await file.GetBasicPropertiesAsync()).Size <= 1024 * 1024 * 1000)
            {
                await using var readStream = await file.OpenReadAsync();
                var data = parser(readStream);
                replacer(data);
                WeakReferenceMessenger.Default.Send(new CSVUploadedMessage());
            }
            else
            {
                throw new Exception("File exceeded 1GB limit.");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private void RefreshDashboard()
    {
        if (!TryBuildSelectedDate(out var date, out var dateOnly))
        {
            SetNoData("Invalid date selection.");
            return;
        }

        List<double> heatDemands;
        try
        {
            heatDemands = _graphDataRepository.GetHeatDemandForDay(date);
        }
        catch
        {
            SetNoData("Heat demand data not available.");
            return;
        }

        if (heatDemands.Count == 0)
        {
            SetNoData("No heat demand data for the selected date.");
            return;
        }

        HeatDemand = SelectedHour < heatDemands.Count ? heatDemands[SelectedHour] : 0;
        if (HeatDemand <= 0)
        {
            SetNoData("No heat demand for the selected hour.");
            return;
        }

        var units = _graphDataRepository.GetProductionUnits().ToList();
        if (units.Count == 0)
        {
            SetNoData("No production units available.");
            return;
        }

        var hourSlot = new HourSlot(dateOnly, SelectedHour);

        double electricityPrice =
            _graphDataRepository.GetElectricityPriceForHour(dateOnly, SelectedHour);

        var results = _optimizer.Optimize(
            HeatDemand, 
            units, 
            hourSlot, 
            electricityPrice,
            MaintenanceSchedule.GetSchedule()

        );

        var summaries = new ObservableCollection<DashboardUnitResult>();
        var totalHeat = 0d;
        var totalCost = 0d;
        var totalCo2 = 0d;

        var unitLookup = units
            .GroupBy(unit => unit.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        foreach (var result in results)
        {
            if (!unitLookup.TryGetValue(result.UnitName, out var unit))
            {
                continue;
            }

            var costRate = NetProductionCostCalculator.ComputeNetProductionCost(
                unit, 
                hourSlot, 
                electricityPrice
            );

            var netCost = result.HeatProduced * costRate;
            var co2 = result.HeatProduced * (unit.Co2Emissions ?? 0);

            summaries.Add(new DashboardUnitResult(
                unit.Name,
                result.HeatProduced,
                costRate,
                netCost,
                co2));

            totalHeat += result.HeatProduced;
            totalCost += netCost;
            totalCo2 += co2;
        }

        ActiveUnits = summaries;
        ActiveUnitsCount = summaries.Count;
        ActiveUnitsSummary = summaries.Count == 0
            ? "No active units."
            : string.Join(", ", summaries.Select(unit => unit.UnitName));
        ShowEmptyState = summaries.Count == 0;

        TotalHeatProduced = totalHeat;
        TotalNetProductionCost = totalCost;
        TotalCo2Emissions = totalCo2;
        NetCostPerMwh = totalHeat > 0 ? totalCost / totalHeat : 0;

        StatusMessage = summaries.Count == 0
            ? "Optimizer ran, but no units were dispatched."
            : "Dashboard updated.";
    }

    private bool TryBuildSelectedDate(out DateTime date, out DateOnly dateOnly)
    {
        date = default;
        dateOnly = default;

        try
        {
            date = new DateTime(SelectedYear, SelectedMonth, SelectedDay);
            dateOnly = new DateOnly(SelectedYear, SelectedMonth, SelectedDay);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void SetNoData(string message)
    {
        StatusMessage = message;
        ActiveUnits = new ObservableCollection<DashboardUnitResult>();
        ActiveUnitsCount = 0;
        ActiveUnitsSummary = "No active units.";
        ShowEmptyState = true;
        HeatDemand = 0;
        TotalHeatProduced = 0;
        TotalNetProductionCost = 0;
        TotalCo2Emissions = 0;
        NetCostPerMwh = 0;
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

    public void Receive(CSVUploadedMessage message)
    {
        RefreshAvailableDates();
        RefreshDashboard();
    }
}

public sealed class DashboardUnitResult
{
    public DashboardUnitResult(string unitName, double heatProduced, double costRate, double netCost, double co2Emissions)
    {
        UnitName = unitName;
        HeatProduced = heatProduced;
        CostRate = costRate;
        NetCost = netCost;
        Co2Emissions = co2Emissions;
    }

    public string UnitName { get; }
    public double HeatProduced { get; }
    public double CostRate { get; }
    public double NetCost { get; }
    public double Co2Emissions { get; }
}