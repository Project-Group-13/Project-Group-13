using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Xml.Schema;
using Microsoft.Data.Sqlite;
using System.Text.Json;
using Heat_Production_Optimization.Data;
using System.Security;
using System.Threading.Tasks;

namespace Heat_Production_Optimization.Services;

public class APIService
{
    private DatabaseConnector connection;

    public APIService(DatabaseConnector conn)
    {
        connection = conn;
    }

    public async Task LoadData()
    {
        try
        {
            if(!CheckData()) return;

            using var client = new HttpClient();

            var jsonDoc = await client.GetFromJsonAsync<JsonDocument>(
                "https://api.energidataservice.dk/dataset/DayAheadPrices?filter={%22PriceArea%22:[%22DK1%22]}&start=2025-10-01&end=2026-01-19");

            if (jsonDoc == null)
                return;

            var data = JsonSerializer.Deserialize<List<APIRecord>>(jsonDoc.RootElement.GetProperty("records"));

            if (data == null)
                return;

            using var conn = connection.GetConnection();
            conn.Open();

            using var command = conn.CreateCommand();

            command.CommandText = @"UPDATE SourceData SET ElectricityPrice = $elecPrice WHERE TimeFrom=$timeFrom";

            var elecPrice = command.CreateParameter();
            elecPrice.ParameterName = "$elecPrice";
            command.Parameters.Add(elecPrice);

            var timeFrom = command.CreateParameter();
            timeFrom.ParameterName = "$timeFrom";
            command.Parameters.Add(timeFrom);

            foreach (var item in data)
            {
                if(item.TimeDK.Minute != 0) continue;

                elecPrice.Value = item.DayAheadPriceDKK;
                timeFrom.Value = item.TimeDK;

                command.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private bool CheckData()
    {
        using var conn = connection.GetConnection();

        conn.Open();

        using var command = conn.CreateCommand();

        command.CommandText = "SELECT COUNT(*) FROM SourceData WHERE ElectricityPrice IS NULL;";

        var result = (long) (command.ExecuteScalar() ?? -1);

        return result > 0;
    }

    private record APIRecord(double DayAheadPriceDKK, DateTime TimeDK);
}