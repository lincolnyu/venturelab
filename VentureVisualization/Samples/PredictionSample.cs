using VentureCommon.Helpers;

namespace VentureVisualization.Samples
{
    public class PredictionSample : ExpertParser.Prediction, IFutureSample
    {
        #region IFutureSample members

        #region ISample members

        /// <summary>
        ///  This indicates number of (working) days since previous
        /// </summary>
        public double Step { get; set; }

        public double Offset { get; set; }

        #endregion

        #endregion
    }
}
