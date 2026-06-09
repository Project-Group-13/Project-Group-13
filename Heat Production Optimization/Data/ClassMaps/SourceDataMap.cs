using CsvHelper.Configuration;
using Heat_Production_Optimization.Models;

namespace Heat_Production_Optimization.Data;

public sealed class SourceDataMap : ClassMap<SourceData>
{
    public SourceDataMap()
    {
        // here we have to use Parameter() instead of map, since in Model.SourceData
        // we do not have settable properties, so Map() does not find anything to write
        // throwing an error
        Parameter("timeFrom").Name("Time from").TypeConverterOption.Format("dd.MM.yyyy HH:mm");
        Parameter("timeTo").Name("Time to").TypeConverterOption.Format("dd.MM.yyyy HH:mm");
        Parameter("heatDemand").Name("Heat Demand");
        Parameter("period").Name("Period");
    }
}