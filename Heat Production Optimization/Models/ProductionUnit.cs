namespace Heat_Production_Optimization.Models
{
    public class ProductionUnit
    {
        public int UnitId { get; set; }
        public string Name { get; set; } = "";

        public double MaxHeat { get; set; }      // MW
        public double ProductionCost { get; set; } // DkK/MWh
        public double? Co2Emissions { get; set; }  // kg/MWh

        public double? EnergyRate { get; set; }
        public double? MaxElectricity { get; set; }

        public string EnergyType { get; set; } = "";
        public string ImagePath { get; set; } = "";
        public int? GridId { get; set; }

        public string UnitType { get; set; } = "";
    }

}
