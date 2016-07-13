using System.Collections.Generic;
using System.Linq;
using VentureLab.Prediction;
using VentureLab.QbGaussianMethod.Cores;
using VentureLab.QbGuassianMethod.Helpers;

namespace VentureLab.QbGaussianMethod.Predictors
{
    public class ConfinedGaussianPredictor : IPredictor
    {
        public void GetExpectedY(IList<double> zeroedOutput, IList<double> input, IEnumerable<IPoint> points) => ConfinedGaussian.GetExpectedYFast(zeroedOutput, input, points.Cast<GaussianRegulatedCore>());

        public void GetExpectedYY(IList<double> zeroedOutput, IList<double> input, IEnumerable<IPoint> points) => ConfinedGaussian.GetExpectedYYFast(zeroedOutput, input, points.Cast<GaussianRegulatedCore>());
    }
}
