using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        ///  Normalize the core (before weight applied) based on the precision factors
        /// </summary>
        public double Normalizer { get; set; }

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
                dd *= K[i];
                sum += dd;
            }
            return Math.Exp(sum);
        }

        public double B(IList<double> x)
        {
            var a = A(x);
            return Weight * Normalizer * Math.Pow(a, M);
        }

        public void UpdateLp()
        {
            var prod = Enumerable.Aggregate(L, (a, b) => a * b);
            Lp = Math.Sqrt(Math.Abs(prod));
        }

        public void UpdateNormalizer()
        {
            // the PI term is omitted as we just want to normalize a constant
            var ml = M - OutputLength;
            var lprod = Enumerable.Aggregate(L, (a, b) => a * b);
            var kprod = Enumerable.Aggregate(K, (a, b) => a * b);
            Normalizer = Math.Sqrt(Math.Abs(ml * kprod)) * Lp;
        }

        /// <summary>
        ///  Use equi-distal method to set the cores
        /// </summary>
        /// <param name="cores">The cores</param>
        public static void SetCoreParameters(ICollection<GaussianRegulatedCore> cores)
        {
            var firstCore = cores.First();
            var outputLen = firstCore.OutputLength;
            var inputLen = firstCore.InputLength;
            var m = firstCore.M; // we assume M's are the same
            var coreCountFactor = Math.Pow(cores.Count - 1, 2.0 / (inputLen+outputLen) );
            coreCountFactor *= Math.Log(2.0);
            // output
            for (var i = 0; i < outputLen; i++)
            {
                var outputMin = cores.Min(c => c.Output[i]);
                var outputMax = cores.Max(c => c.Output[i]);
                var outputSpan = outputMax - outputMin;
                var l = -coreCountFactor / (outputSpan * outputSpan);
                foreach (var c in cores)
                {
                    c.L[i] = l;
                }
            }
            // input
            for (var i = 0; i < inputLen; i++)
            {
                var inputMin = cores.Min(c => c.Input[i]);
                var inputMax = cores.Max(c => c.Input[i]);
                var inputSpan = inputMax - inputMin;
                var k = -coreCountFactor / (m * inputSpan * inputSpan);
                foreach (var c in cores)
                {
                    c.K[i] = k;
                }
            }
            foreach (var c in cores)
            {
                c.UpdateLp();
                c.UpdateNormalizer();
            }
        }
    }
}
