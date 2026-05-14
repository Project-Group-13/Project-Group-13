using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;


namespace Heat_Production_Optimization.Data;

public class CSVParser
{
    public static List<T> Parse<T, TMap>(Stream stream) 
    where T : class
    where TMap : ClassMap<T>
    {
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Context.RegisterClassMap<TMap>();
        
        return csv.GetRecords<T>().ToList();
    }
}