using Microsoft.Data.Sqlite;

namespace Heat_Production_Optimization.Data;

public static class DatabaseInitializer
{
    public static void EnsureCreated()
    {
        using var connection = new DatabaseConnector().GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS SourceData (
                Id               INTEGER PRIMARY KEY AUTOINCREMENT,
                PeriodType       TEXT CHECK (PeriodType = 'winter' OR PeriodType = 'summer'),
                TimeFrom         DATETIME NOT NULL,
                TimeTo           DATETIME NOT NULL,
                HeatDemand       DECIMAL(5, 3) NOT NULL,
                ElectricityPrice DECIMAL(7, 3) NOT NULL,
                UNIQUE (PeriodType, TimeFrom)
            );

            CREATE TABLE IF NOT EXISTS Grid (
                GridId         INTEGER PRIMARY KEY,
                GridName       TEXT NOT NULL,
                BuildingCount  INTEGER NOT NULL,
                City           TEXT NOT NULL,
                ImagePath      TEXT
            );

            CREATE TABLE IF NOT EXISTS ProductionUnits (
                UnitId         INT PRIMARY KEY,
                UnitName       TEXT NOT NULL,
                MaxHeat        DECIMAL(4, 1) NOT NULL,
                ProductionCost INT NOT NULL,
                CO2Rate        INTEGER,
                EnergyRate     DECIMAL(3, 2),
                MaxElectricity DECIMAL(4, 1),
                EnergyType     TEXT NOT NULL,
                ImagePath      TEXT,
                GridId         INTEGER,
                UnitType       TEXT
            );";

        command.ExecuteNonQuery();

        using var tableInfoCommand = connection.CreateCommand();
        tableInfoCommand.CommandText = "PRAGMA table_info(ProductionUnits);";

        var hasUnitTypeColumn = false;
        using (var reader = tableInfoCommand.ExecuteReader())
        {
            while (reader.Read())
            {
                if (reader.GetString(1) == "UnitType")
                {
                    hasUnitTypeColumn = true;
                    break;
                }
            }
        }

        if (!hasUnitTypeColumn)
        {
            using var alterTableCommand = connection.CreateCommand();
            alterTableCommand.CommandText = "ALTER TABLE ProductionUnits ADD COLUMN UnitType TEXT;";
            alterTableCommand.ExecuteNonQuery();
        }
    }
}
