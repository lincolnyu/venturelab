using System;
using System.Collections.Generic;
using System.Linq;
using VentureLab.Helpers;
using VentureLab.QbGaussianMethod.Helpers;

namespace VentureLab.QbGaussianMethod.Cores
{
    public class GaussianRegulatedCore : Point, IWeightedCore
    {
        public GaussianRegulatedCore(int inputLen, int outputLen, 
            double m, double n, double w = 1.0) : base(inputLen, outputLen)
        {
            K = new double[inputLen];
            L = new double[outputLen];
            Weight = w;
            M = m;
            N = n;
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

        public double N { get; }

        public double Lp { get; private set; }

        public double P => M - N * OutputLength / 2.0;

        public double S => Weight * Normalizer;

        public double A(IList<double> x)
        {
            return E(x, N);
        }

        public double B(IList<double> x)
        {
            return S * E(x, M);
        }

        public double E(IList<double> x, double t)
        {
            var sum = 0.0;
            for (var i = 0; i < InputLength; i++)
            {
                var d = x[i] - Input[i];
                var dd = d * d;
                dd *= K[i];
                sum += dd;
            }
            return Math.Exp(t * sum);
        }

        public void UpdateLp()
        {
            var prod = Enumerable.Aggregate(L, (a, b) => a * b);
            Lp = Math.Sqrt(Math.Abs(prod));
        }

        public void UpdateNormalizer()
        {
            // the PI term is omitted as we just want to normalize a constant
            var kprod = Enumerable.Aggregate(K, (a, b) => a * b);
            var kprodcoeff = Math.Sqrt(Math.Abs(kprod));
            var pcoeff = Math.Pow(P, -InputLength / 2.0);
            Normalizer = pcoeff * kprodcoeff * Lp;
            Normalizer /= Math.Pow(Math.PI, (InputLength + OutputLength) / 2.0);
        }

        /// <summary>
        ///  Use equi-distal method to set the cores
        /// </summary>
        /// <param name="cores">The cores</param>
        public static void SetCoreParameters(IEnumerable<GaussianRegulatedCore> cores, double dropThr = 0.6)
        {
            var firstCore = cores.First();
            var outputLen = firstCore.OutputLength;
            var inputLen = firstCore.InputLength;

            var coreColl = cores as ICollection<GaussianRegulatedCore> ?? cores.ToList();

            var inputDistances = new double[inputLen];
            var outputDistances = new double[outputLen];
            CoreGeometryHelper.InitInputOutputDistances(inputDistances, outputDistances);
            var coresDiscending = coreColl.OrderByDescending(x => x.Weight);
            var first = coresDiscending.First();
            var primaryCores = coresDiscending.TakeWhile(x => x.Weight >= dropThr * first.Weight).ToList<IPoint>();
            primaryCores.GetMeanCompsMinDistance(inputDistances, outputDistances);

            var m = firstCore.M; // we assume M's are the same
            var n = firstCore.N;
            var coreCountFactor = Math.Log(2.0);
                        
            // output
            for (var i = 0; i < outputLen; i++)
            {
                var outputSpan = outputDistances[i] / 2;
                var l = -coreCountFactor / (outputSpan * outputSpan);
                foreach (var c in coreColl)
                {
                    c.L[i] = l;
                }
            }

            // input
            for (var i = 0; i < inputLen; i++)
            {
                var inputSpan = inputDistances[i] / 2;
                var k = -coreCountFactor / ((m - 0.5 * n) * inputSpan * inputSpan);
                foreach (var c in coreColl)
                {
                    c.K[i] = k;
                }
            }
            var maxNormalizer = 0.0;
            foreach (var c in coreColl)
            {
                c.UpdateLp();
                c.UpdateNormalizer();
                if (c.Normalizer > maxNormalizer) maxNormalizer = c.Normalizer;
            }
            foreach (var c in coreColl)
            {
                c.Normalizer /= maxNormalizer;
            }
        }
    }
}
