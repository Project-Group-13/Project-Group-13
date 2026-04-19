using Dispatching.Models;

namespace Dispatching.Dispatching
{
    public static class NetProductionCostCalculator
    {
        public static double ComputeNetProductionCost(ProductionUnit unit, HourSlot hour)
        {
            return unit.ProductionCostDKKPerMWh;
        }
    }
}