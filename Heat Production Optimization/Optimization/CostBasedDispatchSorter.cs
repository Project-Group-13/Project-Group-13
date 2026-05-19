using System.Collections.Generic;
using System.Linq;
using Heat_Production_Optimization.Models;

namespace Heat_Production_Optimization.Optimization
{
    public static class CostBasedDispatchSorter
    {
        public static List<ProductionUnit> SortByNetProductionCost(
            IEnumerable<DispatchCandidate> candidates)
        {
            return candidates.OrderBy(c => c.NetProductionCost).ThenBy(c => c.Unit.Name).Select(c => c.Unit).ToList();
        }
    }
}