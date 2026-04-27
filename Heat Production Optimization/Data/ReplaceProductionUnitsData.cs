using System;
using System.Collections.Generic;
using Heat_Production_Optimization.Models;

namespace Heat_Production_Optimization.Data;

public static class ReplaceProductionUnitsData
{
    public static void ReplaceAll(List<ProductionUnit> data)
    {
        using var conn = new DatabaseConnector().GetConnection();
        conn.Open();

        using var transaction = conn.BeginTransaction();

        using (var deleteCommand = conn.CreateCommand())
        {
            deleteCommand.Transaction = transaction;
            deleteCommand.CommandText = "DELETE FROM ProductionUnits;";
            deleteCommand.ExecuteNonQuery();
        }

        using var insertCommand = conn.CreateCommand();
        insertCommand.Transaction = transaction;
        insertCommand.CommandText = @"
            INSERT INTO ProductionUnits (UnitId, UnitName, MaxHeat, ProductionCost, CO2Rate, EnergyRate, MaxElectricity, EnergyType, ImagePath, GridId)
            VALUES ($unitId, $unitName, $maxHeat, $productionCost, $co2Rate, $energyRate, $maxElectricity, $energyType, $imagePath, $gridId);";

        var unitId = insertCommand.CreateParameter();
        unitId.ParameterName = "$unitId";
        insertCommand.Parameters.Add(unitId);

        var unitName = insertCommand.CreateParameter();
        unitName.ParameterName = "$unitName";
        insertCommand.Parameters.Add(unitName);

        var maxHeat = insertCommand.CreateParameter();
        maxHeat.ParameterName = "$maxHeat";
        insertCommand.Parameters.Add(maxHeat);

        var productionCost = insertCommand.CreateParameter();
        productionCost.ParameterName = "$productionCost";
        insertCommand.Parameters.Add(productionCost);

        var co2Rate = insertCommand.CreateParameter();
        co2Rate.ParameterName = "$co2Rate";
        insertCommand.Parameters.Add(co2Rate);

        var energyRate = insertCommand.CreateParameter();
        energyRate.ParameterName = "$energyRate";
        insertCommand.Parameters.Add(energyRate);

        var maxElectricity = insertCommand.CreateParameter();
        maxElectricity.ParameterName = "$maxElectricity";
        insertCommand.Parameters.Add(maxElectricity);

        var energyType = insertCommand.CreateParameter();
        energyType.ParameterName = "$energyType";
        insertCommand.Parameters.Add(energyType);

        var imagePath = insertCommand.CreateParameter();
        imagePath.ParameterName = "$imagePath";
        insertCommand.Parameters.Add(imagePath);

        var gridId = insertCommand.CreateParameter();
        gridId.ParameterName = "$gridId";
        insertCommand.Parameters.Add(gridId);

        foreach (var unit in data)
        {
            unitId.Value = unit.UnitId;
            unitName.Value = unit.Name;
            maxHeat.Value = unit.MaxHeat;
            productionCost.Value = unit.ProductionCost;
            co2Rate.Value = unit.Co2Emissions.HasValue ? unit.Co2Emissions.Value : DBNull.Value;
            energyRate.Value = unit.EnergyRate.HasValue ? unit.EnergyRate.Value : DBNull.Value;
            maxElectricity.Value = unit.MaxElectricity.HasValue ? unit.MaxElectricity.Value : DBNull.Value;
            energyType.Value = unit.EnergyType;
            imagePath.Value = unit.ImagePath is not null ? unit.ImagePath : DBNull.Value;
            gridId.Value = unit.GridId.HasValue ? unit.GridId.Value : DBNull.Value;
            insertCommand.ExecuteNonQuery();
        }

        transaction.Commit();
    }
}
