using System;
using System.Collections.Generic;
using System.Linq;

namespace GaussianCore.Classification
{
    public static class DigioClassifier
    {
        #region Nested types

        public class CodeCoreList
        {
            public string Code { get; set; }

            public List<CoreExtension> Cores { get; private set; } = new List<CoreExtension>();

            public void SortCores()
            {
                Cores.Sort(CoreInputMeanComparer.Instance);
            }
        }

        public class CoreExtensionAndDist : CoreExtension
        {
            public CoreExtensionAndDist(CoreExtension ce, double dist)
            {
                Core = ce.Core;
                InputSum = ce.InputSum;
                DistanceIndicator = dist;
            }

            /// <summary>
            ///  Could be square distance or distance or other, up to the user
            /// </summary>
            public double DistanceIndicator { get; private set; }
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="codeCoreLists"></param>
        /// <param name="ndintol">normalised with Math.Sqrt(sqdist / n)</param>
        /// <param name="ndouttol"></param>
        /// <returns></returns>
        public static double[][] GetWeights(IList<CodeCoreList> codeCoreLists, double ndintol, double ndouttol, double weightbase)
        {
            var res = new double[codeCoreLists.Count - 1][];
            for (var i = 0; i < codeCoreLists.Count - 1; i++)
            {
                res[i] = new double[codeCoreLists.Count - i - 1];
                for (var j = i + 1; j < codeCoreLists.Count; j++)
                {
                    var w = GetWeight(codeCoreLists[i], codeCoreLists[j], ndintol, ndouttol, weightbase);
                    res[i][j - i - 1] = w;
                }
            }
            return res;
        }

        public static double GetWeight(CodeCoreList ccl1, CodeCoreList ccl2, double ndintol, double ndouttol, double weightbase)
        {
            if (ccl2.Cores.Count <ccl1.Cores.Count)
            {
                return GetWeightSmallFirst(ccl2, ccl1, ndintol, ndouttol, weightbase);
            }
            return GetWeightSmallFirst(ccl1, ccl2, ndintol, ndouttol, weightbase);
        }

        public static double GetWeightSmallFirst(CodeCoreList ccl1, CodeCoreList ccl2, double ndintol, double ndouttol, double weightbase)
        {
            var inputLen = ccl1.Cores[0].Core.CentersInput.Count;
            var outputLen = ccl1.Cores[0].Core.CentersOutput.Count;
            var point = 0.0;
            foreach (var c in ccl1.Cores)
            {
                var c2s = Search(c, ccl2, ndintol);
                foreach (var c2 in c2s)
                {
                    var sqdin = c2.DistanceIndicator;
                    var sqdout = c.Core.GetOutputSquareDistance(c2.Core);
                    var ndin = Math.Sqrt(sqdin / inputLen);
                    var ndout = Math.Sqrt(sqdout / outputLen);
                    point = ChangePoint(point, ndin, ndout, ndintol, ndouttol);
                }
            }
            return Math.Pow(weightbase, point);
        }

        public static double ChangePoint(double point, double ndin, double ndout,
            double ndintol, double ndouttol, double inremain = 0.8)
        {
            var k = Math.Log(2) / (ndouttol * ndouttol);
            var a = Math.Exp(-k * ndout * ndout) - 1;

            var b = (inremain - 1) * ndin / ndintol;
            var delta = a * b ;
            point += delta;
            return point;
        }

        public static IEnumerable<CoreExtensionAndDist> Search(CoreExtension ce, CodeCoreList ccl, double ndintol)
        {
            var n = ce.Core.CentersInput.Count;
            var meanTolerance = n * ndintol;
            var sqdtol = meanTolerance * ndintol;

            var index = ccl.Cores.BinarySearch(ce, CoreInputMeanComparer.Instance);

            if (index < 0)
            {
                index = -index;
            }

            for (var i = index; i >= 0; i--)
            {
                var sqd = ce.Core.GetInputSquareDistance(ccl.Cores[i].Core);
                if (sqd > sqdtol)
                {
                    break;
                }
                yield return new CoreExtensionAndDist(ccl.Cores[i], sqd);
            }

            for (var i = index + 1; i < ccl.Cores.Count; i++)
            {
                var sqd = ce.Core.GetInputSquareDistance(ccl.Cores[i].Core);
                if (sqd > sqdtol)
                {
                    break;
                }
                yield return new CoreExtensionAndDist(ccl.Cores[i], sqd);
            }
        }

        public static IEnumerable<CoreExtensionAndDist> Search (ICore core, CodeCoreList ccl, double ndintol)
        {
            var n = core.CentersInput.Count;
            var meanTolerance = Math.Sqrt(n) * ndintol;
            var coreSum = core.CentersInput.Sum();
            var ce = new CoreExtension { Core = core, InputSum = coreSum };
            return Search(ce, ccl, ndintol);
        }

        #endregion
    }
}
