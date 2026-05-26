using Heat_Production_Optimization.Models;
using System;

namespace Heat_Production_Optimization.Optimization
{
    public static class NetProductionCostCalculator
    {
        public static double ComputeNetProductionCost(ProductionUnit unit, HourSlot hour, double electricityPrice)
        {

            if (unit.UnitType == "CHP")
            {

                var baseCost = unit.ProductionCost - (unit.EnergyRate ?? 0) * electricityPrice;

                var operatingCost = 300;
                var maxProfit = 50;

                var cost = baseCost + operatingCost;

                // dynamic penalty based on electricity price
                cost += 0.15 * electricityPrice;

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