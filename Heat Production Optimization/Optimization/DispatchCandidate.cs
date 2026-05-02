using Heat_Production_Optimization.Models;

namespace Heat_Production_Optimization.Optimization
{
    public class DispatchCandidate
    {
        public ProductionUnit Unit { get; }
        public double NetCost { get; }

        public DispatchCandidate(ProductionUnit unit, double netCost)
        {
            Unit = unit;
            NetCost = netCost;
        }
    }
}