using VentureLab.QbClustering;
using VentureLab.QbGaussianMethod.Cores;
using VentureLab.QbGaussianMethod.Predictors;

namespace VentureLab.Asx
{
    public class StockPoint : GaussianRegulatedCore, IStrainPoint
    {
        public class Manager : ConfinedGaussianPredictor, IPointFactory
        {
            public double M { get; set; }

            public IPoint Create() => new StockPoint(SampleAccessor.InputCount, SampleAccessor.OutputCount, M);
        }

        public StockPoint(int inputLen, int outputLen, double m, double w = 1) : base(inputLen, outputLen, m, w)
        {
        }

        public double Indicator { get; set; }

        public int CompareTo(IStrainPoint other) => Indicator.CompareTo(other.Indicator);
    }
}
