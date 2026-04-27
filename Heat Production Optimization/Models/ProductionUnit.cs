namespace Heat_Production_Optimization.Models;

public class ProductionUnit
{
    public int UnitId { get; set; }
    public string Name { get; set; } = "";
    public double MaxHeat { get; set; }
    public double ProductionCost { get; set; }
    public double? Co2Emissions { get; set; }
    public double? EnergyRate { get; set; }
    public double? MaxElectricity { get; set; }
    public string EnergyType { get; set; } = "";
    public string? ImagePath { get; set; }
    public int? GridId { get; set; }
}
