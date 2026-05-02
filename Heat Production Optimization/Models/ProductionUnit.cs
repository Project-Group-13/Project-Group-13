namespace Heat_Production_Optimization.Models
{
    public class ProductionUnit
    {
        public string Name { get; set; } = "";

        public double MaxHeat { get; set; }      // MW
        public double ProductionCost { get; set; } // DkK/MWh
        public double Co2Emissions { get; set; }  // kg/MWh

        public ProductionUnitType UnitType { get; set; }

        //Electricity produced per MWh heat
        public double ElectricityProducedPerHeat { get; set; }

        // Electricity consumed per MWh heat
        public double ElectricityConsumedPerHeat { get; set; }
    }

}
