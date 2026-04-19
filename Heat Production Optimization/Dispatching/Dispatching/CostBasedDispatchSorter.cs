using System.Collections.Generic;
using System.Linq;
using Dispatching.Models;

namespace Dispatching.Dispatching
{
    public static class CostBasedDispatchSorter
    {
        public static List<ProductionUnit> SortByNetProductionCost(
            IEnumerable<DispatchCandidate> candidates)
        {
            return candidates.OrderBy(c => c.NetProductionCost).ThenBy(c => c.Unit.Id).Select(c => c.Unit).ToList();
        }
    }
}