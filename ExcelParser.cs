using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using ExcelDataReader;

public class ExcelHeatDataParser
{
    public Dictionary<DateTime, DailyData> LoadExcelData(
        string path,
        out int winterRowsParsed,
        out int summerRowsParsed)
    {
        winterRowsParsed = 0;
        summerRowsParsed = 0;

        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        var days = new Dictionary<DateTime, DailyData>();
        var culture = new CultureInfo("da-DK");

        using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration
            {
                UseHeaderRow = false
            }
        });

        var table = dataSet.Tables[0];

        for (int r = 0; r < table.Rows.Count; r++)
        {
            var row = table.Rows[r];

            // WINTER: columns 1,3,4
            if (TryAddRecord(row, 1, 3, 4, culture, days))
                winterRowsParsed++;

            // SUMMER: columns 6,8,9
            if (TryAddRecord(row, 6, 8, 9, culture, days))
                summerRowsParsed++;
        }

        return days;
    }

    private static bool TryAddRecord(
        DataRow row,
        int timeCol,
        int demandCol,
        int priceCol,
        CultureInfo culture,
        Dictionary<DateTime, DailyData> days)
    {
        // Ensure row has enough columns
        int max = Math.Max(timeCol, Math.Max(demandCol, priceCol));
        if (row.ItemArray.Length <= max)
            return false;

        // Parse timestamp
        if (!TryParseDateTime(row[timeCol], culture, out var timestamp))
            return false;

        // Parse numerics
        if (!TryParseDouble(row[demandCol], culture, out var heatDemand))
            return false;

        if (!TryParseDouble(row[priceCol], culture, out var electricityPrice))
            return false;

        var day = timestamp.Date;
        var hour = timestamp.Hour;

        if (!days.TryGetValue(day, out var daily))
        {
            daily = new DailyData { Date = day };
            days[day] = daily;
        }

        daily.Hours[hour] = new HourlyData
        {
            Timestamp = timestamp,
            HeatDemand = heatDemand,
            ElectricityPrice = electricityPrice
        };

        return true;
    }

    private static bool TryParseDateTime(object cell, CultureInfo culture, out DateTime value)
    {
        if (cell is DateTime dt)
        {
            value = dt;
            return true;
        }

        if (cell is double d)
        {
            value = DateTime.FromOADate(d);
            return true;
        }

        var s = cell?.ToString()?.Trim();
        if (DateTime.TryParse(s, culture, DateTimeStyles.None, out var parsed))
        {
            value = parsed;
            return true;
        }

        value = default;
        return false;
    }

    private static bool TryParseDouble(object cell, CultureInfo culture, out double value)
    {
        if (cell is double d)
        {
            value = d;
            return true;
        }

        var s = cell?.ToString()?.Trim();
        if (double.TryParse(s, NumberStyles.Any, culture, out var parsed))
        {
            value = parsed;
            return true;
        }

        value = default;
        return false;
    }
}

public class HourlyData
{
    public DateTime Timestamp { get; set; }
    public double HeatDemand { get; set; }
    public double ElectricityPrice { get; set; }
}

public class DailyData
{
    public DateTime Date { get; set; }
    public Dictionary<int, HourlyData> Hours { get; set; } = new Dictionary<int, HourlyData>();
}