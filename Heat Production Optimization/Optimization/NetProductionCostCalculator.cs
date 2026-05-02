using Heat_Production_Optimization.Models;

namespace Heat_Production_Optimization.Optimization
{
    public static class NetProductionCostCalculator
    {
        public static double ComputeNetProductionCost(ProductionUnit unit, HourSlot hour, double electricityPrice)
        {
            switch (unit.UnitType)
            {
                case ProductionUnitType.HeatOnlyBoiler:
                    return unit.ProductionCost;

                case ProductionUnitType.CHP:
                    return unit.ProductionCost - unit.ElectricityProducedPerHeat * electricityPrice;

                case ProductionUnitType.ElectricBoiler:
                    return unit.ProductionCost + unit.ElectricityConsumedPerHeat * electricityPrice;

                default:
                    return unit.ProductionCost;
            }
        }
    }
}