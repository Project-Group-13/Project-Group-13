using System;
using System.Collections.Generic;
using Dispatching.Models;
using Dispatching.Dispatching;

namespace Dispatching
{
    class Program
    {
        static void Main()
        {
            var units = new List<ProductionUnit>
            {
                new ProductionUnit("GB1", 3.0, 510),
                new ProductionUnit("GB2", 2.0, 586),
                new ProductionUnit("GB3", 4.0, 580),
                new ProductionUnit("OB1", 6.0, 690)
            };

            var hour = new HourSlot(new DateOnly(2026, 1, 5), 6);

            var dispatchOrder = CostBasedDispatcher.GetDispatchOrderForHour(units, hour);

            Console.WriteLine($"Dispatch priority for {hour} (cheapest first):");

            foreach (var unit in dispatchOrder)
            {
                Console.WriteLine($"{unit.Id} - {unit.ProductionCostDKKPerMWh} DKK/MWh");
            }
        }
    }
}