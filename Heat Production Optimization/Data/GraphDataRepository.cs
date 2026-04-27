using System.Collections.Generic;
using Heat_Production_Optimization.Models;

namespace Heat_Production_Optimization.Data
{
    public class GraphDataRepository
    {
        public List<ProductionUnit> GetProductionUnits()
        {
            return new List<ProductionUnit>
            {
                new ProductionUnit
                {
                    Name = "GB1",
                    MaxHeat = 3.0,
                    ProductionCost = 510,
                    Co2Emissions = 132
                },
                new ProductionUnit
                {
                    Name = "GB2",
                    MaxHeat = 2.0,
                    ProductionCost = 540,
                    Co2Emissions = 134
                },
                new ProductionUnit
                {
                    Name = "GB3",
                    MaxHeat = 4.0,
                    ProductionCost = 580,
                    Co2Emissions = 136
                },
                new ProductionUnit
                {
                    Name = "OB1",
                    MaxHeat = 6.0,
                    ProductionCost = 690,
                    Co2Emissions = 147
                }
            };
        }
    }
}