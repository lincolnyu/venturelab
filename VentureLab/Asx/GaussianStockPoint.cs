using VentureLab.Prediction;
using VentureLab.QbClustering;
using VentureLab.QbGaussianMethod.Cores;
using VentureLab.QbGaussianMethod.Predictors;

namespace VentureLab.Asx
{
    public class GaussianStockPoint : GaussianRegulatedCore, IStrainPoint
    {
        public class Manager : ConfinedGaussianPredictor, IPointManager
        {
            public double M { get; set; }

            public IPoint Create() => new GaussianStockPoint(SampleAccessor.InputCount, SampleAccessor.OutputCount, M);
        }

        public GaussianStockPoint(int inputLen, int outputLen, double m, double w = 1) : base(inputLen, outputLen, m, w)
        {
        }

        public double Indicator { get; set; }

        public int CompareTo(IStrainPoint other) => Indicator.CompareTo(other.Indicator);
    }
}
