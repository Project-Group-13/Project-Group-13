using System.Collections.Generic;
using System.Linq;

namespace Heat_Production_Optimization.Optimization
{
    public static class CostBasedDispatchSorter
    {
        public static List<DispatchCandidate> SortByNetProductionCost(IEnumerable<DispatchCandidate> candidates)
        {
            return candidates.OrderBy(c => c.NetCost).ToList();
        }
    }
}