using System;

namespace Heat_Production_Optimization.Models;

public class SourceData(DateTime timeFrom, DateTime timeTo, double heatDemand, double electricityPrice, string period)
{
    public DateTime TimeFrom { get; } = timeFrom;
    public DateTime TimeTo { get; } = timeTo;
    public double HeatDemand { get; } = heatDemand;
    public double ElectricityPrice { get; } = electricityPrice;
    public string Period { get; } = period;
}