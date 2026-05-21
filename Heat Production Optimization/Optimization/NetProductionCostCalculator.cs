using Heat_Production_Optimization.Models;

namespace Heat_Production_Optimization.Optimization
{
    public static class NetProductionCostCalculator
    {
        public static double ComputeNetProductionCost(ProductionUnit unit, HourSlot hour, double electricityPrice)
        {
            if(unit.UnitType == "CHP")
            {
                return unit.ProductionCost
                    - (unit.EnergyRate ?? 0) * electricityPrice;
            }
            else if(unit.UnitType == "ElectricBoiler")
            {
                return unit.ProductionCost
                    + (unit.EnergyRate ?? 0) * electricityPrice;
            }

            return unit.ProductionCost;
        }
    }
}