using System.Collections.Generic;
using Heat_Production_Optimization.Models;

namespace Heat_Production_Optimization.Optimization
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