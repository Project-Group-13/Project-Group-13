namespace Dispatching.Models
{
    public class ProductionUnit
    {
        public string Id { get; }
        public double MaxHeatMW { get; }
        public double ProductionCostDKKPerMWh { get; }

        public ProductionUnit(string id, double maxHeatMW, double productionCost)
        {
            Id = id;
            MaxHeatMW = maxHeatMW;
            ProductionCostDKKPerMWh = productionCost;
        }
    }
}
