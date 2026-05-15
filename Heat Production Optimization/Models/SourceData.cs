using System;

namespace Heat_Production_Optimization.Models;

public class SourceData(DateTime timeFrom, DateTime timeTo, double heatDemand, string period)
{
    public DateTime TimeFrom { get; } = timeFrom;
    public DateTime TimeTo { get; } = timeTo;
    public double HeatDemand { get; } = heatDemand;
    public string Period { get; } = period;
}