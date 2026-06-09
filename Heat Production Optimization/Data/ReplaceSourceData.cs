using System;
using System.Collections.Generic;
using Heat_Production_Optimization.Models;
using Microsoft.Data.Sqlite;

namespace Heat_Production_Optimization.Data;

public static class ReplaceSourceData
{
    private const string InsertSql = @"
        INSERT INTO SourceData (PeriodType, TimeFrom, TimeTo, HeatDemand, ElectricityPrice)
        VALUES ($periodType, $timeFrom, $timeTo, $heatDemand, $electricityPrice);";

    public static void ReplaceAll(List<SourceData> data) =>
        ReplaceData.Execute("SourceData", InsertSql, PrepareParameters, BindValues, data);

    private static void PrepareParameters(SqliteCommand cmd)
    {
        foreach (var name in new[] { "$periodType", "$timeFrom", "$timeTo", "$heatDemand", "$electricityPrice" })
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            cmd.Parameters.Add(p);
        }
    }

    private static void BindValues(SqliteCommand cmd, SourceData row)
    {
        cmd.Parameters["$periodType"].Value = row.Period;
        cmd.Parameters["$timeFrom"].Value = row.TimeFrom;
        cmd.Parameters["$timeTo"].Value = row.TimeTo;
        cmd.Parameters["$heatDemand"].Value = row.HeatDemand;
        cmd.Parameters["$electricityPrice"].Value = DBNull.Value;
    }
}
