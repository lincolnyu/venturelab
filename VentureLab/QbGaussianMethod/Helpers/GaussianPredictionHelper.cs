using System.Linq;
using VentureLab.Asx;
using VentureLab.Prediction;
using VentureLab.QbGaussianMethod.Cores;
using static VentureLab.Asx.StockManager;
using static VentureLab.QbGaussianMethod.Helpers.PredictionCommon;

namespace VentureLab.QbGaussianMethod.Helpers
{
    public static class GaussianPredictionHelper
    {
        public static int MinPointCount = 500;

        public static bool Predict(StockManager stockManager, IPointManager pointManager, StockItem item, GetItemIndexCallback giicb, double[] y, double[] yy)
        {
            var points = stockManager.PreparePrediction(item).Cast<GaussianRegulatedCore>().ToList();
            if (points.Count < MinPointCount)
            {
                return false;
            }
            GaussianRegulatedCore.SetCoreParameters(points);
            var index = giicb(item);
            var input = item.SampleInput(pointManager, index);
            pointManager.GetExpectedY(y, input.StrainPoint.Input, points);
            pointManager.GetExpectedYY(yy, input.StrainPoint.Input, points);
            return true;
        }
    }
}
