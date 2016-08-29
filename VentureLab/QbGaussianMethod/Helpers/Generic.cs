using System;
using System.Collections.Generic;
using System.Linq;
using VentureLab.Prediction;
using VentureLab.QbGaussianMethod.Cores;

namespace VentureLab.QbGaussianMethod.Helpers
{
    public static class Generic
    {
        public delegate double VectorToScalar(IList<double> x);

        public static IEnumerable<VectorToScalar> BigB(IEnumerable<VectorToScalar> aa, IList<VectorToScalar> bb, IEnumerable<IList<double>> ll, int outputLen)
        {
            var absum = CSum(aa, bb, ll);
            for (var i = 0; i < bb.Count; i++)
            {
                var b = bb[i];
                VectorToScalar bigb = x => Math.Pow(Math.PI, -outputLen / 2) * b(x) / absum(x);
                yield return bigb;
            }
        }
        public static IEnumerable<VectorToScalar> BigB(IEnumerable<VectorToScalar> aa, IList<VectorToScalar> bb, IEnumerable<IList<double>> ll)
        {
            var outputLen = ll.First().Count;
            return BigB(aa, bb, ll, outputLen);
        }

        /// <summary>
        ///  Return the Ci(x) for all possible i's
        /// </summary>
        /// <param name="aa">ai(x) (a rsub i(x)) for all i's</param>
        /// <param name="bb">bi(x) (b rsub i(x)) for all i's</param>
        /// <param name="ll">lij(x) (l rsub ij (x)) for all i's and j's</param>
        /// <returns></returns>
        /// <remarks>
        ///  See 'Quanben Gaussian Method' formula 1.10
        /// </remarks>
        public static IEnumerable<VectorToScalar> Cc(IEnumerable<VectorToScalar> aa, IEnumerable<VectorToScalar> bb, IEnumerable<IList<double>> ll)
        {
            var eaa = aa.GetEnumerator();
            var ebb = bb.GetEnumerator();
            var ell = ll.GetEnumerator();
            while (eaa.MoveNext() && ebb.MoveNext() && ell.MoveNext())
            {
                var a = eaa.Current;
                var b = ebb.Current;
                var l = ell.Current;
                VectorToScalar c = x =>
                {
                    var outputLen = l.Count;
                    var ab = b(x) * Math.Pow(a(x), -outputLen / 2.0);
                    var lprod = 1.0;
                    for (var j = 0; j < outputLen; j++)
                    {
                        var lij = l[j];
                        lprod *= -lij;
                    }
                    return ab / Math.Sqrt(lprod);
                };
                yield return c;
            }
        }

        public static VectorToScalar CSum(IEnumerable<VectorToScalar> aa, IEnumerable<VectorToScalar> bb, IEnumerable<IList<double>> ll)
        {
            VectorToScalar absumFunc = x =>
            {
                var cc = Cc(aa, bb, ll);
                var absum = cc.Sum(c => c(x));
                return absum;
            };
            return absumFunc;
        }

        public static void ZeroList(IList<double> y)
        {
            for (var i = 0; i < y.Count; i++)
            {
                y[i] = 0.0;
            }
        }

        public static void GetExpectedY(IList<double> zeroedY, IList<double> x, IEnumerable<IPoint> samples, IEnumerable<VectorToScalar> aa, IEnumerable<VectorToScalar> bb, IEnumerable<IList<double>> ll, int outputLen)
        {
            var sumc = 0.0;
            var cc = Cc(aa, bb, ll);
            var ecc = cc.GetEnumerator();
            var esamples = samples.GetEnumerator();
            while (ecc.MoveNext() && esamples.MoveNext())
            {
                var c = ecc.Current;
                var ci = c(x);
                var yi = esamples.Current;
                for (var k = 0; k < outputLen; k++)
                {
                    var yik = yi.Output[k];
                    zeroedY[k] += yik * ci;
                }
                sumc += ci;
            }
            for (var k = 0; k < outputLen; k++)
            {
                zeroedY[k] /= sumc;
            }
        }

        public static void GetExpectedY(IList<double> zeroedY, IList<double> x, IEnumerable<IPoint> samples, IEnumerable<VectorToScalar> aa, IEnumerable<VectorToScalar> bb, IEnumerable<IList<double>> ll)
        {
            var outputLen = ll.First().Count;
            GetExpectedY(zeroedY, x, samples, aa, bb, ll, outputLen);
        }

        public static void GetExpectedYY(IList<double> zeroedYY, IList<double> x, IEnumerable<IPoint> samples, IEnumerable<VectorToScalar> aa, IEnumerable<VectorToScalar> bb, IEnumerable<IList<double>> ll, int outputLen)
        {
            var sumc = 0.0;
            var cc = Cc(aa, bb, ll);
            var ecc = cc.GetEnumerator();
            var esamples = samples.GetEnumerator();
            var eaa = aa.GetEnumerator();
            var ell = ll.GetEnumerator();
            while (eaa.MoveNext() && ecc.MoveNext() && ell.MoveNext() && esamples.MoveNext())
            {
                var c = ecc.Current;
                var ci = c(x);
                var yi = esamples.Current;
                var a = eaa.Current;
                var ai = a(x);
                var l = ell.Current;
                for (var k = 0; k < outputLen; k++)
                {
                    var yik = yi.Output[k];
                    var yik2 = yik * yik;
                    var t = yik2 - 0.5 / (ai * l[k]);
                    zeroedYY[k] += t * ci;
                }
                sumc += ci;
            }
            for (var k = 0; k < outputLen; k++)
            {
                zeroedYY[k] /= sumc;
            }
        }
        
        public static void GetExpectedYY(IList<double> zeroedY, IList<double> x, IEnumerable<IPoint> samples, IEnumerable<VectorToScalar> aa, IEnumerable<VectorToScalar> bb, IEnumerable<IList<double>> ll)
        {
            var outputLen = ll.First().Count;
            GetExpectedYY(zeroedY, x, samples, aa, bb, ll, outputLen);
        }

        public static double GetStrength(IList<double> x, IEnumerable<ICore> cores, IEnumerable<IPoint> points, IEnumerable<VectorToScalar> aa, IEnumerable<VectorToScalar> bb, IEnumerable<IList<double>> ll)
        {
            IPoint maxPoint = null;
            double maxCx = 0.0;
            var epoints = points.GetEnumerator();
            var ecores = cores.GetEnumerator();
            while (epoints.MoveNext() && ecores.MoveNext())
            {
                var c = ecores.Current;
                if (c.MaxCx > maxCx)
                {
                    maxCx = c.MaxCx;
                    maxPoint = epoints.Current;
                }
            }
            var maxCorePx = GetPx(maxPoint.Input, cores, aa, bb, ll);
            var px = GetPx(x, cores, aa, bb, ll);
            return px / maxCorePx;
        }
        
        public static double GetPx(IList<double> x, IEnumerable<ICore> cores, IEnumerable<VectorToScalar> aa, IEnumerable<VectorToScalar> bb, IEnumerable<IList<double>> ll)
        {
            var cc = Cc(aa, bb, ll);
            var ecc = cc.GetEnumerator();
            var ecores = cores.GetEnumerator();
            var num = 0.0;
            var den = 0.0;
            while (ecc.MoveNext() && ecores.MoveNext())
            {
                var c = ecc.Current;
                var ci = c(x);
                var core = ecores.Current;
                num += ci;
                var integral = core.Integral;
                den += integral;
            }
            return num / den;
        }

        public static void Predict(IResult result, IList<double> x, IEnumerable<IPoint> samples,
            IEnumerable<ICore> cores, IEnumerable<VectorToScalar> aa, IEnumerable<VectorToScalar> bb, IEnumerable<IList<double>> ll, int outputLen)
        {
            var zeroedYY = result.YY;
            var zeroedY = result.Y;
            var cc = Cc(aa, bb, ll);
            var sumc = 0.0;
            var num = 0.0;
            var den = 0.0;
            IPoint maxPoint = null;
            double maxCx = 0.0;
            var ecc = cc.GetEnumerator();
            var esamples = samples.GetEnumerator();
            var ecores = cores.GetEnumerator();
            var eaa = aa.GetEnumerator();
            var ell = ll.GetEnumerator();
            while (eaa.MoveNext() && ecc.MoveNext() && ell.MoveNext() && esamples.MoveNext()
                && ecores.MoveNext())
            {
                var c = ecc.Current;
                var ci = c(x);
                var yi = esamples.Current;
                var a = eaa.Current;
                var ai = a(x);
                var l = ell.Current;
                var core = ecores.Current;
                for (var k = 0; k < outputLen; k++)
                {
                    var yik = yi.Output[k];
                    var yik2 = yik * yik;
                    var t = yik2 - 0.5 / (ai * l[k]);
                    zeroedYY[k] += t * ci;
                    zeroedY[k] += yik * ci;
                }
                sumc += ci;
                num += ci;
                var integral = core.Integral;
                den += integral;
                if (core.MaxCx > maxCx)
                {
                    maxCx = core.MaxCx;
                    maxPoint = esamples.Current;
                }
            }
            for (var k = 0; k < outputLen; k++)
            {
                zeroedYY[k] /= sumc;
                zeroedY[k] /= sumc;
            }
            var maxCorePx = GetPx(maxPoint.Input, cores, aa, bb, ll);
            var px = GetPx(x, cores, aa, bb, ll);
            result.Strength = px / maxCorePx;
        }

        public static void Predict(IResult result, IList<double> x, IEnumerable<IPoint> samples, IEnumerable<ICore> cores, IEnumerable<VectorToScalar> aa, IEnumerable<VectorToScalar> bb, IEnumerable<IList<double>> ll)
        {
            var outputLen = ll.First().Count;
            Predict(result, x, samples, cores, aa, bb, ll, outputLen);
        }
    }
}
