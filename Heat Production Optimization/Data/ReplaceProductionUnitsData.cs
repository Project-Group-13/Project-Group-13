using System;
using System.Collections.Generic;
using Heat_Production_Optimization.Models;
using Microsoft.Data.Sqlite;

namespace Heat_Production_Optimization.Data;

public static class ReplaceProductionUnitsData
{
    private const string InsertSql = @"
        INSERT INTO ProductionUnits (UnitId, UnitName, MaxHeat, ProductionCost, CO2Rate, EnergyRate, MaxElectricity, EnergyType, ImagePath, GridId, UnitType)
        VALUES ($unitId, $unitName, $maxHeat, $productionCost, $co2Rate, $energyRate, $maxElectricity, $energyType, $imagePath, $gridId, $unitType);";

    public static void ReplaceAll(List<ProductionUnit> data) =>
        ReplaceData.Execute("ProductionUnits", InsertSql, PrepareParameters, BindValues, data);

    private static void PrepareParameters(SqliteCommand cmd)
    {
        foreach (var name in new[] { "$unitId", "$unitName", "$maxHeat", "$productionCost",
                                     "$co2Rate", "$energyRate", "$maxElectricity", "$energyType",
                                     "$imagePath", "$gridId", "$unitType" })
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            cmd.Parameters.Add(p);
        }
    }

    private static void BindValues(SqliteCommand cmd, ProductionUnit unit)
    {
        cmd.Parameters["$unitId"].Value = unit.UnitId;
        cmd.Parameters["$unitName"].Value = unit.Name;
        cmd.Parameters["$maxHeat"].Value = unit.MaxHeat;
        cmd.Parameters["$productionCost"].Value = unit.ProductionCost;
        cmd.Parameters["$co2Rate"].Value = (object?)unit.Co2Emissions ?? DBNull.Value;
        cmd.Parameters["$energyRate"].Value = (object?)unit.EnergyRate ?? DBNull.Value;
        cmd.Parameters["$maxElectricity"].Value = (object?)unit.MaxElectricity ?? DBNull.Value;
        cmd.Parameters["$energyType"].Value = unit.EnergyType;
        cmd.Parameters["$imagePath"].Value = (object?)unit.ImagePath ?? DBNull.Value;
        cmd.Parameters["$gridId"].Value = (object?)unit.GridId ?? DBNull.Value;
        cmd.Parameters["$unitType"].Value = unit.UnitType;
    }
}
