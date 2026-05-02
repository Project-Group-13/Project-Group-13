using System;
using System.Collections.Generic;
using Heat_Production_Optimization.Models;

namespace Heat_Production_Optimization.Optimization
{
    public class Optimizer
    {
        public List<OptimizerResult> Optimize(double heatDemand, List<ProductionUnit> units, HourSlot hour, double electricityPrice)
        {
            var results = new List<OptimizerResult>();

            // Task 4.2: cost-based dispatch order
            var orderedUnits = CostBasedDispatcher.GetDispatchOrderForHour(units, hour, electricityPrice);

            // Task 4.1: heat allocation
            double remainingDemand = heatDemand;

            foreach (var unit in orderedUnits)
            {
                if (remainingDemand <= 0)
                    break;

                double heatProduced = Math.Min(remainingDemand, unit.MaxHeat);
                remainingDemand -= heatProduced;

                results.Add(new OptimizerResult
                {
                    UnitName = unit.Name,
                    HeatProduced = heatProduced
                });
            }

            return results;
        }
    }
}