using System.Collections.Generic;
using System.Linq;
using Heat_Production_Optimization.Models;

namespace Heat_Production_Optimization.Optimization
{
    public static class CostBasedDispatcher
    {
        public static List<ProductionUnit> GetDispatchOrderForHour(
            IEnumerable<ProductionUnit> units,
            HourSlot hour, 
            double electricityType)
        {
            var candidates = 
                DispatchCandidateBuilder.Build(units, hour, electricityType);

            return candidates.
                OrderBy(c => c.NetCost)
                .Select(c => c.Unit)
                .ToList();
        }
    }
}