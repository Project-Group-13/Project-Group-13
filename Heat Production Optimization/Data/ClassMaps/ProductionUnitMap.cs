using CsvHelper.Configuration;
using Heat_Production_Optimization.Models;

namespace Heat_Production_Optimization.Data;

public sealed class ProductionUnitMap : ClassMap<ProductionUnit>
{
    public ProductionUnitMap()
    {
        Map(m => m.UnitId).Name("UnitId");
        Map(m => m.Name).Name("UnitName");
        Map(m => m.MaxHeat).Name("MaxHeat");
        Map(m => m.ProductionCost).Name("ProductionCost");
        Map(m => m.Co2Emissions).Name("CO2Rate");
        Map(m => m.EnergyRate).Name("EnergyRate");
        Map(m => m.MaxElectricity).Name("MaxElectricity");
        Map(m => m.EnergyType).Name("EnergyType");
        Map(m => m.ImagePath).Name("ImagePath");
        Map(m => m.GridId).Name("GridId");
    }
}
