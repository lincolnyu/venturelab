using System;
using System.Collections.Generic;
using System.Linq;
using VentureLab.Prediction;
using VentureLab.QbGaussianMethod.Cores;
using static VentureLab.QbGaussianMethod.Helpers.Generic;

namespace VentureLab.QbGuassianMethod.Helpers
{
    public static class ConfinedGaussian
    {
        public static double GetStrengthThruGeneric(IList<double> x, IEnumerable<GaussianRegulatedCore> cores) => GetStrength(x, cores, cores.Select<GaussianRegulatedCore, VectorToScalar>(c => c.A), cores.Select<GaussianRegulatedCore, VectorToScalar>(c => c.B), cores.Select(c => c.L));
        

        public static void GetExpectedYThruGeneric(IList<double> zeroedY, IList<double> x, IEnumerable<GaussianRegulatedCore> cores) =>
            GetExpectedY(zeroedY, x, cores.Select(c=>c.Point), cores.Select<GaussianRegulatedCore, VectorToScalar>(c => c.A), cores.Select<GaussianRegulatedCore, VectorToScalar>(c => c.B), cores.Select(c => c.L));

        public static void GetExpectedYYThruGeneric(IList<double> zeroedY, IList<double> x, IEnumerable<GaussianRegulatedCore> cores) => GetExpectedYY(zeroedY, x, cores.Select(c => c.Point), cores.Select<GaussianRegulatedCore, VectorToScalar>(c => c.A), cores.Select<GaussianRegulatedCore, VectorToScalar>(c => c.B), cores.Select(c => c.L));

        public static void PredictThruGeneric(IResult result, IList<double> x, IEnumerable<GaussianRegulatedCore> cores) => Predict(result, x, cores.Select(c => c.Point), cores, cores.Select<GaussianRegulatedCore, VectorToScalar>(c => c.A), cores.Select<GaussianRegulatedCore, VectorToScalar>(c => c.B), cores.Select(c => c.L));

        /// <summary>
        ///  This is up to the user to call to remedy NaN outputs possibly caused by zero EP (due to far distance between input and any cores, which may be an unstable and problematic prediction case and should be taken note of anyway)
        /// </summary>
        /// <param name="x">The input</param>
        /// <param name="cores">All the cores</param>
        /// <param name="vars">The core variables that the caller should take hold of</param>
        public static void OffsetEp(IList<double> x, IEnumerable<GaussianRegulatedCore> cores,
            IEnumerable<GaussianRegulatedCoreVariables> vars)
        {
            var minEps = double.MinValue;
            foreach (var c in cores)
            {
                var eps = c.GetEpShortfall(x);
                if (eps > minEps) minEps = eps;
            }
            foreach (var v in vars)
            {
                v.EpOffset = -minEps;
            }
        }

        public static void PredictFast(IResult result, IList<double> x, IEnumerable<GaussianRegulatedCore> cores)
        {
            var wasum = 0.0;
            var num = 0.0;
            var den = 0.0;
            var maxMaxCx = 0.0;
            var coreCount = 0;
            foreach (var c in cores)
            {
                var cy = c.Point.Output;
                var s = c.S / c.Variables.Lp;
                var ep = c.E(x, c.Constants.P);
                var pp = Math.Pow(c.Constants.P, x.Count / 2.0);
                num += c.Weight * c.Variables.Kp * pp * ep;
                den += c.Weight;
                var sep = s * ep;
                var sd = s * Math.Pow(ep, (c.Constants.P - c.Constants.N) / c.Constants.P);
                for (var k = 0; k < result.YY.Count; k++)
                {
                    result.Y[k] += cy[k] * sep;
                    result.YY[k] += cy[k] * cy[k] * sep - 0.5 * sd / c.L[k];
                }
                wasum += sep;
                var maxCx = c.MaxCx / c.Integral; // TODO optimize
                if (maxCx > maxMaxCx) maxMaxCx = maxCx;
                coreCount++;
            }
            for (var k = 0; k < result.YY.Count; k++)
            {
                result.Y[k] /= wasum;
                result.YY[k] /= wasum;
            }
            var coef = Math.Pow(Math.PI, -x.Count / 2.0);
            result.Strength = coef * num * coreCount / (den * maxMaxCx);
        }

        public static double GetStrengthFast(IList<double> x, IEnumerable<GaussianRegulatedCore> cores)
        {
            var num = 0.0;
            var den = 0.0;
            var maxMaxCx = 0.0;
            var coreCount = 0;
            foreach (var c in cores)
            {
                var ep = c.E(x, c.Constants.P);
                var pp = Math.Pow(c.Constants.P, x.Count / 2.0);
                num += c.Weight * c.Variables.Kp * pp * ep;
                den += c.Weight;
                var maxCx = c.MaxCx / c.Integral; // TODO optimize
                if (maxCx > maxMaxCx) maxMaxCx = maxCx;
                coreCount++;
            }
            var coef = Math.Pow(Math.PI, -x.Count / 2.0);
            var res = coef * num * coreCount / (den * maxMaxCx);
            return res;
        }

        public static void GetExpectedYFast(IList<double> zeroedY, IList<double> x, IEnumerable<GaussianRegulatedCore> cores)
        {
            var wasum = 0.0;
            foreach (var c in cores)
            {
                var cy = c.Point.Output;
                var s = c.S / c.Variables.Lp;
                var ep = c.E(x, c.Constants.P);
                var sep = s * ep;
                for (var k = 0; k < zeroedY.Count; k++)
                {
                    zeroedY[k] += cy[k] * sep;
                }
                wasum += sep;
            }
            for (var k = 0; k < zeroedY.Count; k++)
            {
                zeroedY[k] /= wasum;
            }
        }

        public static void GetExpectedYYFast(IList<double> zeroedYY, IList<double> x, IEnumerable<GaussianRegulatedCore> cores)
        {
            var wasum = 0.0;
            foreach (var c in cores)
            {
                var cy = c.Point.Output;
                var s = c.S / c.Variables.Lp;
                var ep = c.E(x, c.Constants.P);
                var sep = s * ep;
                var sd = s * Math.Pow(ep, (c.Constants.P - c.Constants.N) / c.Constants.P);
                for (var k = 0; k < zeroedYY.Count; k++)
                {
                    zeroedYY[k] += cy[k] * cy[k] * sep - 0.5 * sd / c.L[k];
                }
                wasum += sep;
            }
            for (var k = 0; k < zeroedYY.Count; k++)
            {
                zeroedYY[k] /= wasum;
            }
        }
    }
}
