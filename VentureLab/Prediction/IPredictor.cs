using System.Collections.Generic;
using VentureLab.QbGaussianMethod.Cores;

namespace VentureLab.Prediction
{
    public interface IPredictor
    {
        void Predict(IResult result, IList<double> input, IEnumerable<ICore> cores);

        double GetStrength(IList<double> input, IEnumerable<ICore> cores);

        void GetExpectedY(IList<double> zeroedOutput, IList<double> input, IEnumerable<ICore> cores);

        void GetExpectedYY(IList<double> zeroedOutput, IList<double> input, IEnumerable<ICore> cores);
    }
}
