namespace Heat_Production_Optimization.Models
{
        /// Name of the production unit (e.g. GB1, GB2, OB1)
        /// Heat produced by the unit in the selected hour (MW or MWh depending on convention)
    public sealed record OptimizerResult(string UnitName, double HeatProduced);
}
