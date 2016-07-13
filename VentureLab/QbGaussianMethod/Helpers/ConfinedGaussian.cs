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
                var w = c.Weight / c.Lp;
                var ap = Math.Pow(c.A(x), c.P);
                var wap = w * ap;
                for (var k = 0; k < zeroedY.Count; k++)
                {
                    zeroedY[k] += cy[k] * wap;
                }
                wasum += wap;
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
                var w = c.Weight / c.Lp;
                var a = c.A(x);
                var ap = Math.Pow(a, c.P);
                var wap = w * ap;
                for (var k = 0; k < zeroedY.Count; k++)
                {
                    var t = cy[k] * cy[k] - 0.5 / (a * c.L[k]);
                    zeroedY[k] += t * wap;
                }
                wasum += wap;
            }
            for (var k = 0; k < zeroedY.Count; k++)
            {
                zeroedY[k] /= wasum;
            }
        }
    }
}
