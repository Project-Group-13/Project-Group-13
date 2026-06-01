using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Heat_Production_Optimization.Data;
using Heat_Production_Optimization.Models;
using Heat_Production_Optimization.Services;

namespace Heat_Production_Optimization.Tests;

public class CSVParserTests
{
    private static Stream ToStream(string content) =>
        new MemoryStream(Encoding.UTF8.GetBytes(content));

    [Fact]
    public void ParseSourceData_ValidCsv_ReturnsParsedRecords()
    {
        var csv = """
            Time from,Time to,Heat Demand,Period
            01.01.2025 00:00,01.01.2025 01:00,14.5,Winter
            01.01.2025 01:00,01.01.2025 02:00,13.2,Winter
            """;

        var result = CSVParser.Parse<SourceData, SourceDataMap>(ToStream(csv));

        Assert.Equal(2, result.Count);
        Assert.Equal(new DateTime(2025, 1, 1, 0, 0, 0), result[0].TimeFrom);
        Assert.Equal(new DateTime(2025, 1, 1, 1, 0, 0), result[0].TimeTo);
        Assert.Equal(14.5, result[0].HeatDemand);
        Assert.Equal("Winter", result[0].Period);
    }

    [Fact]
    public void ParseSourceData_EmptyCsv_ReturnsEmptyList()
    {
        var csv = "Time from,Time to,Heat Demand,Period\n";

        var result = CSVParser.Parse<SourceData, SourceDataMap>(ToStream(csv));

        Assert.Empty(result);
    }

    [Fact]
    public void ParseProductionUnit_ValidCsv_ReturnsParsedRecords()
    {
        var csv = """
            UnitId,UnitName,MaxHeat,ProductionCost,CO2Rate,EnergyRate,MaxElectricity,EnergyType,ImagePath,GridId,UnitType
            1,GB1,5.0,50.0,215.0,,,,Assets/gb1.jpg,1,GasBoiler
            2,EK1,8.0,75.0,,,3.0,Electric,,1,ElectricBoiler
            """;

        var result = CSVParser.Parse<ProductionUnit, ProductionUnitMap>(ToStream(csv));

        Assert.Equal(2, result.Count);

        var gb1 = result[0];
        Assert.Equal(1, gb1.UnitId);
        Assert.Equal("GB1", gb1.Name);
        Assert.Equal(5.0, gb1.MaxHeat);
        Assert.Equal(50.0, gb1.ProductionCost);
        Assert.Equal(215.0, gb1.Co2Emissions);
        Assert.Null(gb1.EnergyRate);
        Assert.Equal("GasBoiler", gb1.UnitType);

        var ek1 = result[1];
        Assert.Equal(3.0, ek1.MaxElectricity);
        Assert.Null(ek1.Co2Emissions);
        Assert.Equal("ElectricBoiler", ek1.UnitType);
    }

    [Fact]
    public void ParseProductionUnit_EmptyCsv_ReturnsEmptyList()
    {
        var csv = "UnitId,UnitName,MaxHeat,ProductionCost,CO2Rate,EnergyRate,MaxElectricity,EnergyType,ImagePath,GridId,UnitType\n";

        var result = CSVParser.Parse<ProductionUnit, ProductionUnitMap>(ToStream(csv));

        Assert.Empty(result);
    }

    [Fact]
    public void ParseSourceData_SingleRow_ReturnsSingleRecord()
    {
        var csv = """
            Time from,Time to,Heat Demand,Period
            15.06.2025 12:00,15.06.2025 13:00,20.0,Summer
            """;

        var result = CSVParser.Parse<SourceData, SourceDataMap>(ToStream(csv));

        Assert.Single(result);
        Assert.Equal(new DateTime(2025, 6, 15, 12, 0, 0), result[0].TimeFrom);
        Assert.Equal(20.0, result[0].HeatDemand);
        Assert.Equal("Summer", result[0].Period);
    }

    [Fact]
    public void ParseSourceData_InvalidDateFormat_Throws()
    {
        var csv = """
            Time from,Time to,Heat Demand,Period
            2025-01-01,2025-01-01,14.5,Winter
            """;

        Assert.Throws<CsvHelper.TypeConversion.TypeConverterException>(
            () => CSVParser.Parse<SourceData, SourceDataMap>(ToStream(csv)));
    }
}
