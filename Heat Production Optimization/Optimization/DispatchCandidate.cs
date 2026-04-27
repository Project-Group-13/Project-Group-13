using Heat_Production_Optimization.Models;

namespace Heat_Production_Optimization.Models
{
    public class DispatchCandidate
    {
        public ProductionUnit Unit { get; }
        public double NetProductionCost { get; }

        public DispatchCandidate(ProductionUnit unit,double netProductionCost)
        {
            Unit = unit;
            NetProductionCost = netProductionCost;
        }
    }
}