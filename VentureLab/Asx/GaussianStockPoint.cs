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
            public double N { get; set; }

            public IPoint Create() => new GaussianStockPoint(SampleAccessor.InputCount, SampleAccessor.OutputCount, M, N);
        }

        public GaussianStockPoint(int inputLen, int outputLen, double m, double n, double w = 1) : base(inputLen, outputLen, m, n, w)
        {
        }

        public double Indicator { get; set; }

        public int CompareTo(IStrainPoint other) => Indicator.CompareTo(other.Indicator);
    }
}
