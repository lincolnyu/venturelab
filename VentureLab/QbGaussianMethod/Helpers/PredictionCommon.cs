using System;
using VentureLab.Asx;
using VentureLab.Prediction;
using static VentureLab.Asx.StockManager;

namespace VentureLab.QbGaussianMethod.Helpers
{
    public static class PredictionCommon
    {
        public const string ErrorCantLocate = "Couldn't find acceptable date";
        public const string ErrorNotEnoughCores = "Not enough points to be statistically significant.";

        public delegate int GetItemIndexCallback(StockItem item);

        public delegate void PredictDelegate(StockManager stockManager, IPointManager pointManager, StockItem item, GetItemIndexCallback giicb, IPredictionResult result);

        public interface IPredictionResult
        {
            double[] Y { get; }
            double[] YY { get; }
            DateTime Date { get; set; }
            string ErrorMessage { get; set; }
        }

        public class PredictionResult : IPredictionResult
        {
            public PredictionResult(int outLen)
            {
                Y = new double[outLen];
                YY = new double[outLen];
            }

            public DateTime Date
            {
                get; set;
            }

            public string ErrorMessage
            {
                get; set;
            }

            public double[] Y
            {
                get;
            }

            public double[] YY
            {
                get;
            }
        }
    }
}
