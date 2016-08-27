using System;
using System.Collections.Generic;
using System.Linq;
using VentureLab.Prediction;
using VentureLab.QbGaussianMethod.Cores;
using VentureLab.QbGuassianMethod.Helpers;

namespace VentureLab.QbGaussianMethod.Predictors
{
    public class ConfinedGaussianPredictor : IPredictor
    {
        public void GetExpectedY(IList<double> zeroedOutput, IList<double> input, IEnumerable<ICore> cores) => ConfinedGaussian.GetExpectedYFast(zeroedOutput, input, cores.Cast<GaussianRegulatedCore>());

        public void GetExpectedYY(IList<double> zeroedOutput, IList<double> input, IEnumerable<ICore> cores) => ConfinedGaussian.GetExpectedYYFast(zeroedOutput, input, cores.Cast<GaussianRegulatedCore>());

        public double GetStrength(IList<double> input, IEnumerable<ICore> cores) => ConfinedGaussian.GetStrengthFast(input, cores.Cast<GaussianRegulatedCore>());

        public void Predict(IResult result, IList<double> input, IEnumerable<ICore> cores) => ConfinedGaussian.PredictFast(result, input, cores.Cast<GaussianRegulatedCore>());
    }
}
