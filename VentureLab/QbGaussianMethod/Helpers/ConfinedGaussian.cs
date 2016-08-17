using System;
using System.Collections.Generic;
using System.Linq;
using VentureLab.QbGaussianMethod.Cores;
using static VentureLab.QbGaussianMethod.Helpers.Generic;

namespace VentureLab.QbGuassianMethod.Helpers
{
    public static class ConfinedGaussian
    {
        public static void GetExpectedYThruGeneric(IList<double> zeroedY, IList<double> x, IEnumerable<GaussianRegulatedCore> cores)
        {
            GetExpectedY(zeroedY, x, cores, cores.Select<GaussianRegulatedCore, VectorToScalar>(c => c.A), cores.Select<GaussianRegulatedCore, VectorToScalar>(c => c.B), cores.Select(c => c.L));
        }

        public static void GetExpectedYYThruGeneric(IList<double> zeroedY, IList<double> x, IEnumerable<GaussianRegulatedCore> cores)
        {
            GetExpectedYY(zeroedY, x, cores, cores.Select<GaussianRegulatedCore, VectorToScalar>(c => c.A), cores.Select<GaussianRegulatedCore, VectorToScalar>(c => c.B), cores.Select(c => c.L));
        }

        public static void GetExpectedYFast(IList<double> zeroedY, IList<double> x, IEnumerable<GaussianRegulatedCore> cores)
        {
            var wasum = 0.0;
            foreach (var c in cores)
            {
                var cy = c.Output;
                var s = c.S / c.Lp;
                var ep = c.E(x, c.P);
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

        public static void GetExpectedYYFast(IList<double> zeroedY, IList<double> x, IEnumerable<GaussianRegulatedCore> cores)
        {
            var wasum = 0.0;
            foreach (var c in cores)
            {
                var cy = c.Output;
                var s = c.S / c.Lp;
                var e = c.E(x, 1);
                var ep = Math.Pow(e, c.P);
                var sep = s * ep;
                var sd = s * Math.Pow(e, c.P - c.N);
                for (var k = 0; k < zeroedY.Count; k++)
                {
                    zeroedY[k] += cy[k] * cy[k] * sep - 0.5 * sd / c.L[k];
                }
                wasum += sep;
            }
            for (var k = 0; k < zeroedY.Count; k++)
            {
                zeroedY[k] /= wasum;
            }
        }
    }
}
