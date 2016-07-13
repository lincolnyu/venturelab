using System.Collections.Generic;
using VentureLab.QbGaussianMethod.Cores;

namespace VentureLab.Prediction
{
    public interface IPredictor
    {
        void GetExpectedY(IList<double> zeroedOutput, IList<double> input, IEnumerable<IPoint> points);

        void GetExpectedYY(IList<double> zeroedOutput, IList<double> input, IEnumerable<IPoint> points);
    }
}
