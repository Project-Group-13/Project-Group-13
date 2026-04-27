using Heat_Production_Optimization.Models;

namespace Heat_Production_Optimization.Optimization
{
    public static class NetProductionCostCalculator
    {
        public static double ComputeNetProductionCost(ProductionUnit unit, HourSlot hour)
        {
            return unit.ProductionCost;
        }
    }
}