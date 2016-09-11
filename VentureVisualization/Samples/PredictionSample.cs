namespace VentureVisualization.Samples
{
    public class PredictionSample : IFutureSample
    {
        #region IFutureSample members

        #region ISample members

        /// <summary>
        ///  This indicates number of (working) days since previous
        /// </summary>
        public double Step { get; set; }

        public double Offset { get; set; }

        #endregion

        public int Days { get; set; }

        #endregion

        public double Y { get; set; }
        public double StdVar { get; set; }
    }
}
