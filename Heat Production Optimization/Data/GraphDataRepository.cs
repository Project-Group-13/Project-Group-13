using System;
using System.Collections.Generic;
using System.Globalization;
using Heat_Production_Optimization.Models;
using Microsoft.Data.Sqlite;

namespace Heat_Production_Optimization.Data;

public sealed class GraphDataRepository
{
    private readonly DatabaseConnector _connector = new();

    public IReadOnlyList<HeatPoint> GetHeatDemandSeries(int maxPoints = 120)
    {
        var data = new List<HeatPoint>();
        if (!TableExists("SourceData"))
        {
            return data;
        }

        using var connection = _connector.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT TimeFrom, HeatDemand
            FROM SourceData
            WHERE TimeFrom IS NOT NULL AND HeatDemand IS NOT NULL
            ORDER BY TimeFrom
            LIMIT $limit;";
        command.Parameters.AddWithValue("$limit", maxPoints);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var timestamp = ParseDateTime(reader.GetValue(0));
            var heatDemand = ParseDouble(reader.GetValue(1));

            if (timestamp != DateTime.MinValue && !double.IsNaN(heatDemand))
            {
                data.Add(new HeatPoint(timestamp, heatDemand));
            }
        }

        return data;
    }

    public IReadOnlyList<DateOnly> GetAvailableHeatDemandDates()
    {
        var data = new List<DateOnly>();
        if (!TableExists("SourceData"))
        {
            return data;
        }

        using var connection = _connector.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT DISTINCT date(TimeFrom) AS DayBucket
            FROM SourceData
            WHERE TimeFrom IS NOT NULL AND HeatDemand IS NOT NULL
            ORDER BY date(TimeFrom);";

        var seen = new HashSet<DateOnly>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var timestamp = ParseDateTime(reader.GetValue(0));
            if (timestamp == DateTime.MinValue)
            {
                continue;
            }

            var date = DateOnly.FromDateTime(timestamp);
            if (seen.Add(date))
            {
                data.Add(date);
            }
        }

        return data;
    }

    public IReadOnlyList<HeatPoint> GetElectricityPriceSeries(int maxPoints = 120)
    {
        var data = new List<HeatPoint>();
        if (!TableExists("SourceData"))
        {
            return data;
        }

        using var connection = _connector.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT TimeFrom, ElectricityPrice
            FROM SourceData
            WHERE TimeFrom IS NOT NULL AND ElectricityPrice IS NOT NULL
            ORDER BY TimeFrom
            LIMIT $limit;";
        command.Parameters.AddWithValue("$limit", maxPoints);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var timestamp = ParseDateTime(reader.GetValue(0));
            var electricityPrice = ParseDouble(reader.GetValue(1));

            if (timestamp != DateTime.MinValue && !double.IsNaN(electricityPrice))
            {
                data.Add(new HeatPoint(timestamp, electricityPrice));
            }
        }

        return data;
    }

    public IReadOnlyList<HeatPoint> GetEstimatedDailyCostSeries(int maxPoints = 60)
    {
        var data = new List<HeatPoint>();
        if (!TableExists("SourceData") || !TableExists("ProductionUnits"))
        {
            return data;
        }

        using var connection = _connector.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT date(sd.TimeFrom) AS DayBucket,
                   SUM(sd.HeatDemand * avgCost.AvgProductionCost) AS EstimatedDailyCost
            FROM SourceData sd
            CROSS JOIN (
                SELECT AVG(ProductionCost) AS AvgProductionCost
                FROM ProductionUnits
                WHERE ProductionCost IS NOT NULL
            ) avgCost
            WHERE sd.TimeFrom IS NOT NULL AND sd.HeatDemand IS NOT NULL
            GROUP BY date(sd.TimeFrom)
            ORDER BY date(sd.TimeFrom)
            LIMIT $limit;";
        command.Parameters.AddWithValue("$limit", maxPoints);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var timestamp = ParseDateTime(reader.GetValue(0));
            var estimatedCost = ParseDouble(reader.GetValue(1));

            if (timestamp != DateTime.MinValue && !double.IsNaN(estimatedCost))
            {
                data.Add(new HeatPoint(timestamp, estimatedCost));
            }
        }

        return data;
    }

    public IReadOnlyList<UnitCostPoint> GetUnitCostSeries()
    {
        return GetUnitMetricSeries("ProductionCost");
    }

    public IReadOnlyList<UnitCostPoint> GetUnitMaxHeatSeries()
    {
        return GetUnitMetricSeries("MaxHeat");
    }

    public IReadOnlyList<UnitCostPoint> GetUnitEnergyRateSeries()
    {
        return GetUnitMetricSeries("EnergyRate");
    }

    public IReadOnlyList<ProductionUnit> GetProductionUnits()
    {
        var data = new List<ProductionUnit>();
        if (!TableExists("ProductionUnits"))
        {
            return data;
        }

        using var connection = _connector.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT *
            FROM ProductionUnits
            ORDER BY UnitName;";

        using var reader = command.ExecuteReader();
        if (!TryGetOrdinal(reader, "UnitName", out var unitNameOrdinal))
        {
            return data;
        }

        TryGetOrdinal(reader, "MaxHeat", out var maxHeatOrdinal);
        TryGetOrdinal(reader, "ProductionCost", out var productionCostOrdinal);
        var hasCo2 = TryGetOrdinal(reader, "Co2Rate", out var co2Ordinal)
            || TryGetOrdinal(reader, "CO2Rate", out co2Ordinal)
            || TryGetOrdinal(reader, "Co2Rate", out co2Ordinal)
            || TryGetOrdinal(reader, "CO2Rate", out co2Ordinal);
        TryGetOrdinal(reader, "ImagePath", out var imagePathOrdinal);
        TryGetOrdinal(reader, "MaxElectricity", out var maxElectricityOrdinal);


        while (reader.Read())
        {
            var unitName = Convert.ToString(reader.GetValue(unitNameOrdinal), CultureInfo.InvariantCulture);
            if (string.IsNullOrWhiteSpace(unitName))
            {
                continue;
            }

            var maxHeat = maxHeatOrdinal >= 0 ? ParseDouble(reader.GetValue(maxHeatOrdinal)) : double.NaN;
            var productionCost = productionCostOrdinal >= 0 ? ParseDouble(reader.GetValue(productionCostOrdinal)) : double.NaN;
            var imagePath = imagePathOrdinal >= 0 ? Convert.ToString(reader.GetValue(imagePathOrdinal), CultureInfo.InvariantCulture) : string.Empty;
            var co2 = hasCo2 && co2Ordinal >= 0 ? ParseDouble(reader.GetValue(co2Ordinal)) : double.NaN;
            var maxElectricity = maxElectricityOrdinal >= 0 ? ParseDouble(reader.GetValue(maxElectricityOrdinal)) : double.NaN;

            string unitType = unitName switch
            {
                "GM1" => "CHP",
                "EB1" => "Electricboiler",
                _ => "HeatOnly"
            };

            data.Add(new ProductionUnit
            {
                UnitType = unitType,
                Name = unitName,
                MaxHeat = double.IsNaN(maxHeat) ? 0 : maxHeat,
                ProductionCost = double.IsNaN(productionCost) ? 0 : productionCost,
                Co2Emissions = double.IsNaN(co2) ? 0 : co2,
                ImagePath = imagePath ?? string.Empty,
                MaxElectricity = double.IsNaN(maxElectricity) ? null : maxElectricity
            });
        }

        return data;
    }

    private IReadOnlyList<UnitCostPoint> GetUnitMetricSeries(string metricColumn)
    {
        var data = new List<UnitCostPoint>();
        if (!TableExists("ProductionUnits"))
        {
            return data;
        }

        using var connection = _connector.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = $@"
            SELECT UnitName, {metricColumn}
            FROM ProductionUnits
            WHERE UnitName IS NOT NULL AND {metricColumn} IS NOT NULL
            ORDER BY UnitName;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var unitName = reader.GetString(0);
            var value = ParseDouble(reader.GetValue(1));

            if (!double.IsNaN(value))
            {
                data.Add(new UnitCostPoint(unitName, value));
            }
        }

        return data;
    }

    private bool TableExists(string tableName)
    {
        using var connection = _connector.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM sqlite_master WHERE type = 'table' AND name = $name;";
        command.Parameters.AddWithValue("$name", tableName);

        var count = Convert.ToInt32(command.ExecuteScalar());
        return count > 0;
    }

    private static DateTime ParseDateTime(object value)
    {
        if (value is DateTime dateTime)
        {
            return dateTime;
        }

        if (value is long unixLong)
        {
            try
            {
                return DateTimeOffset.FromUnixTimeSeconds(unixLong).LocalDateTime;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        if (value is int unixInt)
        {
            try
            {
                return DateTimeOffset.FromUnixTimeSeconds(unixInt).LocalDateTime;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        if (value is double oaValue)
        {
            try
            {
                return DateTime.FromOADate(oaValue);
            }
            catch
            {
                // Continue to textual parsing.
            }
        }

        var raw = Convert.ToString(value, CultureInfo.InvariantCulture);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return DateTime.MinValue;
        }

        if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var unixParsed))
        {
            try
            {
                return DateTimeOffset.FromUnixTimeSeconds(unixParsed).LocalDateTime;
            }
            catch
            {
                // Continue to textual parsing.
            }
        }

        if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var invariantDate))
        {
            return invariantDate;
        }

        return DateTime.TryParse(raw, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var localDate)
            ? localDate
            : DateTime.MinValue;
    }

    private static double ParseDouble(object value)
    {
        if (value is double parsedDouble)
        {
            return parsedDouble;
        }

        if (value is float parsedFloat)
        {
            return parsedFloat;
        }

        if (value is decimal parsedDecimal)
        {
            return (double)parsedDecimal;
        }

        var raw = Convert.ToString(value, CultureInfo.InvariantCulture);
        if (double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var invariantValue))
        {
            return invariantValue;
        }

        if (double.TryParse(raw, NumberStyles.Any, CultureInfo.CurrentCulture, out var localValue))
        {
            return localValue;
        }

        return double.NaN;
    }

    private static bool TryGetOrdinal(SqliteDataReader reader, string columnName, out int ordinal)
    {
        for (var i = 0; i < reader.FieldCount; i++)
        {
            if (string.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
            {
                ordinal = i;
                return true;
            }
        }

        ordinal = -1;
        return false;
    }
}

public sealed record HeatPoint(DateTime Timestamp, double Value);

public sealed record UnitCostPoint(string UnitName, double Value);
