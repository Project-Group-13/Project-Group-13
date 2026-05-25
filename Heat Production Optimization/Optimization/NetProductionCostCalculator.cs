using Heat_Production_Optimization.Models;
using System;

namespace Heat_Production_Optimization.Optimization
{
    public static class NetProductionCostCalculator
    {
        public static double ComputeNetProductionCost(ProductionUnit unit, HourSlot hour, double electricityPrice)
        {
            if(unit.UnitType == "CHP")
            {
                var cost = unit.ProductionCost - (unit.EnergyRate ?? 0) * electricityPrice;

                cost += 210;

                return Math.Max(cost, -200);
            }
            else if(unit.UnitType == "ElectricBoiler")
            {
                return unit.ProductionCost + (unit.EnergyRate ?? 0) * electricityPrice;
            }

            return unit.ProductionCost;
        }
    }
}