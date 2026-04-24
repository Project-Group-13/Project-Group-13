using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using Heat_Production_Optimization.Model;

namespace Heat_Production_Optimization.Data;

public class CSVParser
{
    public static List<SourceData> Parse(Stream stream)
    {
        using var reader = new StreamReader(stream);

        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        // Skip the three header rows
        csv.Read();
        csv.Read();
        csv.Read();

        List<SourceData> data = new();

        while(csv.Read())
        {
            data.Add(new SourceData(ParseDate(csv.GetField(0)), ParseDate(csv.GetField(1)), csv.GetField<double>(2), csv.GetField<double>(3), "winter"));
            data.Add(new SourceData(ParseDate(csv.GetField(5)), ParseDate(csv.GetField(6)), csv.GetField<double>(7), csv.GetField<double>(8), "summer"));
        }
        
        return data;
    }
    // This is needed due to CultureInfo.InvarianteCulture set above that 
    // expects the datatime to be in MM.dd.yyyy and csv has dd.MM.yyyy
    // so for days 12< it fails  
    private static DateTime ParseDate(string? value) =>
        DateTime.ParseExact(value ?? string.Empty, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
}