using System.Collections.Generic;
using Heat_Production_Optimization.Models;

namespace Heat_Production_Optimization.Optimization
{
    public static class DispatchCandidateBuilder
    {
        public static List<DispatchCandidate> Build(IEnumerable<ProductionUnit> units, HourSlot hour, double electricityPrice)
        {
            var candidates = new List<DispatchCandidate>();

            foreach (var unit in units)
            {
                double netCost = NetProductionCostCalculator.ComputeNetProductionCost(unit, hour, electricityPrice);

                candidates.Add(new DispatchCandidate(unit, netCost));
            }
            return candidates;
        }
    }
}