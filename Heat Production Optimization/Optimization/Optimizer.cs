using System;
using System.Linq;
using System.Collections.Generic;
using Heat_Production_Optimization.Models;

namespace Heat_Production_Optimization.Optimization
{
    public class Optimizer
    {
        public List<OptimizerResult> Optimize(
            double heatDemand,
            List<ProductionUnit> units,
            HourSlot hour,
            double electricityPrice,
            List<MaintenancePeriod> maintenancePeriods)
        {
            var results = new List<OptimizerResult>();

            var currentHour = new DateTime(hour.Date.Year, hour.Date.Month, hour.Date.Day, hour.Hour, 0, 0);
            
            units = units.Where(u => !maintenancePeriods.Any(m => 
                m.UnitName == u.Name && m.IsUnderMaintenance(currentHour)
            )).ToList();

            var orderedUnits =
                CostBasedDispatcher.GetDispatchOrderForHour(units, hour, electricityPrice);

            double remainingDemand = heatDemand;

            foreach (var unit in orderedUnits)
            {
                if (remainingDemand <= 0)
                    break;

                double heatProduced =
                    Math.Min(remainingDemand, unit.MaxHeat);

                remainingDemand -= heatProduced;

                results.Add(new OptimizerResult
                (
                    unit.Name,
                    heatProduced
                ));
            }

            return results;
        }
    }
}