using System;
using System.Collections.Generic;

namespace Heat_Production_Optimization.Models;

public static class MaintenanceSchedule
{
    public static List<MaintenancePeriod> GetSchedule()
    {
        return new List<MaintenancePeriod>
        {
            new MaintenancePeriod
            {
                UnitName = "GB1",
                Start = new DateTime(2026, 1, 7, 0, 0, 0),
                DurationHours = 60
            },
            new MaintenancePeriod
            {
                UnitName = "GB1",
                Start = new DateTime(2025, 9, 10, 0, 0, 0),
                DurationHours = 60
            }
        };
    }
}