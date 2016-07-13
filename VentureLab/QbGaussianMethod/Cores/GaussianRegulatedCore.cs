using System;
using System.Collections.Generic;

namespace VentureLab.QbGaussianMethod.Cores
{
    public class GaussianRegulatedCore : Point, IWeightedCore
    {
        public GaussianRegulatedCore(int inputLen, int outputLen, double m, double w = 1.0) : base(inputLen, outputLen)
        {
            K = new double[inputLen];
            L = new double[outputLen];
            Weight = w;
            M = m;
        }

        /// <summary>
        ///  Weight
        /// </summary>
        public double Weight { get; set; }

        public IList<double> K { get; }

        public IList<double> L { get; }

        public double M { get; }

        public double Lp { get; private set; }

        public double P => M - OutputLength / 2.0;

        public double A(IList<double> x)
        {
            var sum = 0.0;
            for (var i = 0; i < InputLength; i++)
            {
                var d = x[i] - Input[i];
                var dd = d * d;
                sum += dd;
            }
            return Math.Exp(sum);
        }

        public double B(IList<double> x)
        {
            var a = A(x);
            return Weight * Math.Pow(a, M);
        }

        public void UpdateLp()
        {
            var prod = 1.0;
            foreach (var l in L)
            {
                prod *= -l;
            }
            Lp = Math.Sqrt(prod);
        }
    }
}
