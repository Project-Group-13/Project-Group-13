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
                var baseCost = unit.ProductionCost - (unit.EnergyRate ?? 0) * (electricityPrice * 0.9);

                var operatingCost = 50;
                var maxProfit = 500;

                var cost = baseCost + operatingCost;

                double dynamicPenalty = 0.2 * electricityPrice;

                //cost += dynamicPenalty;

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