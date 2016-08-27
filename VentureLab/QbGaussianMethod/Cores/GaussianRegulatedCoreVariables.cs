using System;
using System.Collections.Generic;
using System.Linq;

namespace VentureLab.QbGaussianMethod.Cores
{
    public class GaussianRegulatedCoreVariables
    {
        public GaussianRegulatedCoreVariables(int inputLen, int outputLen)
        {
            K = new double[inputLen];
            L = new double[outputLen];
        }

        public GaussianRegulatedCore Core { get; internal set; }

        public IList<double> K { get; }

        public IList<double> L { get; }

        public double Lp { get; private set; }

        /// <summary>
        ///  Normalize the core (before weight applied) based on the precision factors
        /// </summary>
        public double Normalizer { get; set; }

        public double EpOffset = 0;

        public void UpdateLp()
        {
            var prod = Enumerable.Aggregate(L, (a, b) => a * b);
            Lp = Math.Sqrt(Math.Abs(prod));
        }

        public void UpdateNormalizer()
        {
            // the PI term is omitted as we just want to normalize a constant
            var inputLen = Core.Point.InputLength;
            var outputLen = Core.Point.OutputLength;
            var kprod = Enumerable.Aggregate(K, (a, b) => a * b);
            var kprodcoeff = Math.Sqrt(Math.Abs(kprod));
            var pcoeff = Math.Pow(Core.Constants.P, -inputLen / 2.0);
            Normalizer = pcoeff * kprodcoeff * Lp;
            Normalizer /= Math.Pow(Math.PI, (inputLen + outputLen) / 2.0);
        }
    }
}
