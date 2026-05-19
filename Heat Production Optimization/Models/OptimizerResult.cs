namespace Heat_Production_Optimization.Models
{
    public class OptimizerResult
    {
        /// <summary>
        /// Name of the production unit (e.g. GB1, GB2, OB1)
        /// </summary>
        public string UnitName { get; set; }

        /// <summary>
        /// Heat produced by the unit in the selected hour (MW or MWh depending on convention)
        /// </summary>
        public double HeatProduced { get; set; }
    }
}
