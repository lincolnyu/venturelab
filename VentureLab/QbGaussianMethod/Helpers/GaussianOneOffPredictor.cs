using System.Linq;
using VentureLab.Asx;
using VentureLab.Prediction;
using VentureLab.QbGaussianMethod.Cores;
using VentureLab.QbGuassianMethod.Helpers;
using static VentureLab.Asx.StockManager;
using static VentureLab.QbGaussianMethod.Helpers.PredictionCommon;

namespace VentureLab.QbGaussianMethod.Helpers
{
    public class GaussianOneOffPredictor
    {
        public const int DefaultMinPointCount = 500;

        public GaussianOneOffPredictor(int minPointCount = DefaultMinPointCount)
        {
            MinPointCount = minPointCount;
        }

        public static GaussianOneOffPredictor Instance { get; } = new GaussianOneOffPredictor();

        public int MinPointCount { get; }

        public void Predict(StockManager stockManager, IPointManager pointManager, StockItem item, GetItemIndexCallback giicb, IPredictionResult result)
        {
            var index = giicb(item);
            if (index < 0)
            {
                result.ErrorMessage = ErrorCantLocate;
                return;
            }

            var cores = stockManager.PreparePrediction(item, pointManager).Cast<GaussianRegulatedCore>().ToList();
            if (cores.Count < MinPointCount)
            {
                result.ErrorMessage = ErrorNotEnoughCores;
                return;
            }
            var gf = pointManager as IGaussianCoreFactory;
            var varsets = gf?.GetCoreVariableSets() ?? cores.Select(c => c.Variables).Distinct();
            GaussianRegulatedCore.SetCoreVariables(cores, varsets);
            
            var input = item.SampleInput(pointManager, index);
            var x = input.StrainPoint.Input;
            pointManager.Predict(result, x, cores);

            if (result.Y.All(y=>double.IsNaN(y)) && result.YY.All(yy=>double.IsNaN(yy)))
            {
                ConfinedGaussian.OffsetEp(x, cores, varsets);
                result.Reset();
                pointManager.Predict(result, x, cores);
            }

            result.Date = item.Stock.Data[index].Date;
        }
    }
}
