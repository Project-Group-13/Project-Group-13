using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Microsoft.Data.Sqlite;

namespace Heat_Production_Optimization.Data
{
    public class OptimizerHeatDemand
    {
        private readonly DatabaseConnector _dbService;

        public OptimizerHeatDemand()
        {
            _dbService = new();
        }

        public List<double> GetHeatDemandForDay(DateTime date)
        {
            var heatDemands = new double[24];

            using var connection = _dbService.GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT TimeFrom, HeatDemand
                FROM SourceData
                WHERE date(TimeFrom) = date(@date)
                ORDER BY TimeFrom
            ";

            command.Parameters.AddWithValue("@date", date.Date);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                DateTime timeFrom = reader.GetDateTime(0);
                double heatDemand = reader.GetDouble(1);

                int hour = timeFrom.Hour;
                heatDemands[hour] = heatDemand;
            }

            return new List<double>(heatDemands);
        }
    }
}