using System;

namespace Heat_Production_Optimization.Models;

public class MaintenancePeriod
{
    public string UnitName { get; set; } = "";
    public DateTime Start { get; set; }
    public int DurationHours { get; set; }

    public DateTime End => Start.AddHours(DurationHours);

    public bool IsUnderMaintenance(DateTime hour)
    {
        return hour >= Start && hour < End;
    }
}