using VentureLab.Asx;
using VentureLab.Prediction;
using static VentureLab.Asx.StockManager;

namespace VentureLab.QbGaussianMethod.Helpers
{
    public static class PredictionCommon
    {
        public delegate int GetItemIndexCallback(StockItem item);

        public delegate bool PredictDelegate(StockManager stockManager, IPointManager pointManager, StockItem item, GetItemIndexCallback giicb, double[] y, double[] yy);
    }
}
