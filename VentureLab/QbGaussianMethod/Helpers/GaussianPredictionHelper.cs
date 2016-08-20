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
            var cores = stockManager.PreparePrediction(item, pointManager).Cast<GaussianRegulatedCore>().ToList();
            if (cores.Count < MinPointCount)
            {
                return false;
            }
            var gf = pointManager as IGaussianCoreFactory;
            var varsets = gf?.GetCoreVariableSets() ?? cores.Select(x => x.Variables).Distinct();
            GaussianRegulatedCore.SetCoreVariables(cores, varsets);
            var index = giicb(item);
            var input = item.SampleInput(pointManager, index);
            pointManager.GetExpectedY(y, input.StrainPoint.Input, cores);
            pointManager.GetExpectedYY(yy, input.StrainPoint.Input, cores);
            return true;
        }
    }
}
