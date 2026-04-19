using System.Collections.Generic;
using Dispatching.Models;

namespace Dispatching.Dispatching
{
    public static class CostBasedDispatcher
    {
        public static List<ProductionUnit> GetDispatchOrderForHour(IEnumerable<ProductionUnit> units, HourSlot hour)
        {
            var candidates = DispatchCandidateBuilder.Build(units, hour);

            return CostBasedDispatchSorter.SortByNetProductionCost(candidates);
        }
    }
}