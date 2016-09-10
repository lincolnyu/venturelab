namespace VentureVisualization.Samples
{
    public class PredictionSample : ISample
    {
        /// <summary>
        ///  This indicates number of (working) days
        /// </summary>
        public double Step { get; set; }
        
        public double Y { get; set; }
        public double StdVar { get; set; }

        public double Offset { get; set; }
    }
}
