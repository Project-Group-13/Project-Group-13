using Heat_Production_Optimization.Models;
using System;

namespace Heat_Production_Optimization.Optimization
{
    public static class NetProductionCostCalculator
    {
        public static double ComputeNetProductionCost(ProductionUnit unit, HourSlot hour, double electricityPrice)
        {
            var operatingCost = 250;    // additional running cost
            var maxProfit = 200;        // cap on profit

            if (unit.UnitType == "CHP")
            {
                var cost = unit.ProductionCost - (unit.EnergyRate ?? 0) * electricityPrice;

                cost += operatingCost;

                return Math.Max(cost, -maxProfit);
            }
            else if(unit.UnitType == "ElectricBoiler")
            {
                return unit.ProductionCost + (unit.EnergyRate ?? 0) * electricityPrice;
            }

            return unit.ProductionCost;
        }
    }
}