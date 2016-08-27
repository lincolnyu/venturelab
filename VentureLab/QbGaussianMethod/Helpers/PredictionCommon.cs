using System;
using System.Collections.Generic;
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

        public interface IPredictionResult : IResult
        {
            DateTime Date { get; set; }
            string ErrorMessage { get; set; }
            void Reset();
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

            public IList<double> Y
            {
                get;
            }

            public IList<double> YY
            {
                get;
            }

            public double Strength { get; set; }

            public void Reset()
            {
                for (var i = 0; i < Y.Count; i++) Y[i] = 0;
                for (var i = 0; i < YY.Count; i++) YY[i] = 0;
            }
        }
    }
}
