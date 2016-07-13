using System.Collections.Generic;
using VentureLab.Prediction;
using VentureLab.QbGaussianMethod.Cores;

namespace VentureLab.Asx
{
    /// <summary>
    ///  Expected Ys and Square Ys of a sampled input
    /// </summary>
    public class Expectation
    {
        public double FP1Gain { get; private set; }
        public double FP2Gain { get; private set; }
        public double FP5Gain { get; private set; }
        public double FP10Gain { get; private set; }
        public double FP20Gain { get; private set; }
        public double FP65Gain { get; private set; }

        public double FP1GainSqr { get; private set; }
        public double FP2GainSqr { get; private set; }
        public double FP5GainSqr { get; private set; }
        public double FP10GainSqr { get; private set; }
        public double FP20GainSqr { get; private set; }
        public double FP65GainSqr { get; private set; }

        /// <summary>
        ///  Get expected 1-order output of the sample
        /// </summary>
        /// <param name="sample">The sample of the stock to predict</param>
        /// <param name="points">The related points of the stock</param>
        /// <param name="predictor">The matching predictor</param>
        public void GetExpectedY(SampleAccessor sample, IEnumerable<IPoint> points, IPredictor predictor)
        {
            var input = sample.StrainPoint.Input;
            var y = new double[6];
            predictor.GetExpectedY(y, input, points);
            FP1Gain = y[0];
            FP2Gain = y[1];
            FP5Gain = y[2];
            FP10Gain = y[3];
            FP20Gain = y[4];
            FP65Gain = y[5];
        }

        /// <summary>
        ///  Get expected 2nd-order output of the sample
        /// </summary>
        /// <param name="sample">The sample of the stock to predict</param>
        /// <param name="points">The related points of the stock</param>
        /// <param name="predictor">The matching predictor</param>
        public void GetExpectedYY(SampleAccessor sample, IEnumerable<IPoint> points, IPredictor predictor)
        {
            var input = sample.StrainPoint.Input;
            var yy = new double[6];
            predictor.GetExpectedYY(yy, input, points);
            FP1GainSqr = yy[0];
            FP2GainSqr = yy[1];
            FP5GainSqr = yy[2];
            FP10GainSqr = yy[3];
            FP20GainSqr = yy[4];
            FP65GainSqr = yy[5];
        }
    }
}
