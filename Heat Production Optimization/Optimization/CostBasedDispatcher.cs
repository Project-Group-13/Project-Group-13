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
            double electricityPrice)
        {
            var candidates = 
                DispatchCandidateBuilder.Build(units, hour, electricityPrice);

            return candidates
                .OrderBy(c => c.NetProductionCost)
                .ThenBy(c => c.Unit.Name) // this makes the ordering deterministic for units that have the same netproductioncost
                .Select(c => c.Unit)
                .ToList();
        }
    }
}