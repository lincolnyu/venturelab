using System;
using System.Collections.Generic;
using System.Linq;
using VentureLab.Prediction;
using VentureLab.QbGaussianMethod.Cores;
using VentureLab.QbGaussianMethod.Helpers;
using static VentureLab.QbGaussianMethod.Helpers.Generic;

namespace VentureLab.QbGaussianMethod.Predictors
{
    public class GenericPredictor : IPredictor
    {
        public void GetExpectedY(IList<double> zeroedOutput, IList<double> input, IEnumerable<ICore> cores)
        {
            var points = cores.Cast<IPointWrapper>().Select(x => x.Point);
            var aa = cores.Select(x => (VectorToScalar)x.A);
            var bb = cores.Select(x => (VectorToScalar)x.B);
            var ll = cores.Select(x => x.L);
            Generic.GetExpectedY(zeroedOutput, input, points, aa, bb, ll);
        }

        public void GetExpectedYY(IList<double> zeroedOutput, IList<double> input, IEnumerable<ICore> cores)
        {
            var points = cores.Cast<IPointWrapper>().Select(x => x.Point);
            var aa = cores.Select(x => (VectorToScalar)x.A);
            var bb = cores.Select(x => (VectorToScalar)x.B);
            var ll = cores.Select(x => x.L);
            Generic.GetExpectedYY(zeroedOutput, input, points, aa, bb, ll);
        }

        public double GetStrength(IList<double> input, IEnumerable<ICore> cores)
        {
            throw new NotSupportedException();
        }

        public void Predict(IResult result, IList<double> input, IEnumerable<ICore> cores)
        {
            var points = cores.Cast<IPointWrapper>().Select(x => x.Point);
            var aa = cores.Select(x => (VectorToScalar)x.A);
            var bb = cores.Select(x => (VectorToScalar)x.B);
            var ll = cores.Select(x => x.L);
            Generic.Predict(result, input, points, cores, aa, bb, ll);
        }
    }
}
