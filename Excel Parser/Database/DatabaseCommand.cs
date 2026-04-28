using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

namespace Parser;

public class DatabaseCommand
{
    private readonly string _dbPath;

    public DatabaseCommand()
    {
        _dbPath = $"Data Source={ResolveDatabasePath()}";
    }

    public void Insert(Dictionary<DateTime, DailyData> data)
    {
        string query = @"INSERT OR IGNORE INTO SourceData (PeriodType, TimeFrom, TimeTo, HeatDemand, ElectricityPrice) VALUES 
                            (@PeryodParam, @FromParam, @ToParam, @HeatDemandParam, @ElecPriceParam)";


        using (SqliteConnection connection = new SqliteConnection(_dbPath))
        {
            connection.Open();

            using (SqliteCommand command = new SqliteCommand(query, connection))
            {
                foreach(var entry in data)
                {
                    foreach(var hourData in entry.Value.Hours)
                    {
                        command.Parameters.Clear();

                        var type = hourData.Value.Type;
                        var timeFrom = hourData.Value.Timestamp;
                        var timeTo = hourData.Value.Timestamp.AddHours(1);
                        var heatDemand = hourData.Value.HeatDemand;
                        var elecPrice = hourData.Value.ElectricityPrice;

                        command.Parameters.AddWithValue("@PeryodParam", type);
                        command.Parameters.AddWithValue("@FromParam", timeFrom);
                        command.Parameters.AddWithValue("@ToParam", timeTo);
                        command.Parameters.AddWithValue("@HeatDemandParam", heatDemand);
                        command.Parameters.AddWithValue("@ElecPriceParam", elecPrice);

                        command.ExecuteNonQuery();
                    }
                }
            }
        }

    }

    private static string ResolveDatabasePath()
    {
        var candidates = new List<string>
        {
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Heat Production Optimization", "Data", "database.db"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Data", "database.db"),
            Path.Combine(AppContext.BaseDirectory, "Database", "database.db"),
            Path.Combine(Environment.CurrentDirectory, "Heat Production Optimization", "Data", "database.db"),
            Path.Combine(Environment.CurrentDirectory, "Data", "database.db"),
            Path.Combine(Environment.CurrentDirectory, "Database", "database.db")
        };

        foreach (var path in candidates)
        {
            var fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Heat Production Optimization", "Data", "database.db"));
    }
}