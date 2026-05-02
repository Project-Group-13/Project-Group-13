using System.Collections.Generic;
using Heat_Production_Optimization.Models;

namespace Heat_Production_Optimization.Optimization
{
    public static class CostBasedDispatcher
    {
        public static List<ProductionUnit> GetDispatchOrderForHour(IEnumerable<ProductionUnit> units, HourSlot hour, double electricityType)
        {
            var candidates = DispatchCandidateBuilder.Build(units, hour, electricityType);

            return CostBasedDispatchSorter.SortByNetProductionCost(candidates).ConvertAll(c => c.Unit);
        }
    }
}