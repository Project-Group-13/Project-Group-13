namespace Heat_Production_Optimization.Models;

public class ProductionUnit
{
    public string Name { get; set; } = "";
    public double MaxHeat { get; set; }      // MW
    public double ProductionCost { get; set; } // DkK/MWh
    public double Co2Emissions { get; set; }  // kg/MWh
}