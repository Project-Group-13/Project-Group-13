using Microsoft.Data.Sqlite;

namespace Parser;

public class DatabaseCommand
{
    private string _dbPath = "Data Source=Database/database.db";

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
}