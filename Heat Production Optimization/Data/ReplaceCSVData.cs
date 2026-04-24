using System.Collections.Generic;
using Heat_Production_Optimization.Model;

namespace Heat_Production_Optimization.Data;

public static class ReplaceCSVData
{
    public static void ReplaceAll(List<SourceData> data)
    {
        using var conn = new DatabaseConnector().GetConnection();
        conn.Open();

        // BeginTransaction() allows grouping of multiple SQL statements into a single unit of work
        // This is needed to avoid database data being wiped due to issues with the InsertInto query
        using var transaction = conn.BeginTransaction();

        using (var deleteCommand = conn.CreateCommand())
        {
            deleteCommand.Transaction = transaction;
            deleteCommand.CommandText = "DELETE FROM SourceData;";
            deleteCommand.ExecuteNonQuery();
        }

        using var insertCommand = conn.CreateCommand();
        insertCommand.Transaction = transaction;
        insertCommand.CommandText = @"
            INSERT INTO SourceData (PeriodType, TimeFrom, TimeTo, HeatDemand, ElectricityPrice)
            VALUES ($periodType, $timeFrom, $timeTo, $heatDemand, $electricityPrice);";

        var periodType = insertCommand.CreateParameter();
        periodType.ParameterName = "$periodType";
        insertCommand.Parameters.Add(periodType);

        var timeFrom = insertCommand.CreateParameter();
        timeFrom.ParameterName = "$timeFrom";
        insertCommand.Parameters.Add(timeFrom);

        var timeTo = insertCommand.CreateParameter();
        timeTo.ParameterName = "$timeTo";
        insertCommand.Parameters.Add(timeTo);

        var heatDemand = insertCommand.CreateParameter();
        heatDemand.ParameterName = "$heatDemand";
        insertCommand.Parameters.Add(heatDemand);

        var electricityPrice = insertCommand.CreateParameter();
        electricityPrice.ParameterName = "$electricityPrice";
        insertCommand.Parameters.Add(electricityPrice);

        foreach (var row in data)
        {
            periodType.Value = row.Period;
            timeFrom.Value = row.TimeFrom;
            timeTo.Value = row.TimeTo;
            heatDemand.Value = row.HeatDemand;
            electricityPrice.Value = row.ElectricityPrice;
            insertCommand.ExecuteNonQuery();
        }

        transaction.Commit();
    }
}
