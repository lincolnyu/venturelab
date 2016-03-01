using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GaussianCore.Classification
{
    public static class DigioClassifier
    {
        #region Nested types

        /// <summary>
        ///  A list of cores with the same code
        /// </summary>
        public class CodeCoreList
        {
            /// <summary>
            ///  The code the cores share
            /// </summary>
            public string Code { get; set; }

            /// <summary>
            ///  The list of cores
            /// </summary>
            public List<CoreExtension> Cores { get; private set; } = new List<CoreExtension>();

            /// <summary>
            ///  Sort the cores by mean value of input 
            /// </summary>
            public void SortCores()
            {
                Cores.Sort(CoreInputMeanComparer.Instance);
            }
        }

        /// <summary>
        ///  Core extension with input sum (derived from CoreExtension) and distance information
        /// </summary>
        public class CoreExtensionAndDist : CoreExtension
        {
            /// <summary>
            ///  Instantiates an extension
            /// </summary>
            /// <param name="ce">The core extension with core and input sum</param>
            /// <param name="dist">The distance indicator</param>
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

        private struct SubtotalObject
        {
            public double ScoreSubtotal { get; set; }
            public int CountSubtotal { get; set; }
        }


        #endregion

        #region Properties

        #endregion

        #region Methods

        /// <summary>
        ///  returns the weight from the weight table. the weight table is organised as below
        ///     (0,1), (0,2), (0,3), (0,4), ...
        ///     (1,2), (1,3), (1,4), ...
        ///     (2,3), (2,4), ...
        ///     ...
        /// </summary>
        /// <param name="weights">The weight table</param>
        /// <param name="i">The index of the first core list in the core list list</param>
        /// <param name="j">The index of the second core list in the core list list</param>
        /// <returns>The weight</returns>
        public static double GetWeight(this double[][] weights, int i, int j)
        {
            if (i < j)
            {
                return weights[i][j - i - 1];
            }
            else if (j < i)
            {
                return weights[j][i - j - 1];
            }
            else
            {
                throw new ArgumentException("Getting weight between two identical core lists");
            }
        }


        /// <summary>
        ///  returns the weight table between core lists
        /// </summary>
        /// <param name="codeCoreLists">The core lists</param>
        /// <param name="inputTol">normalised with Math.Sqrt(sqdist / n)</param>
        /// <returns>
        ///  The weight table in the following format
        ///    (0,1), (0,2), (0,3), (0,4), ...
        ///           (1,2), (1,3), (1,4), ...
        ///                  (2,3), (2,4), ...
        ///                          ...
        /// </returns>
        public static double[][] GetWeights(this IList<CodeCoreList> codeCoreLists, double inputTol, TextWriter logWriter = null)
        {
            var res = new double[codeCoreLists.Count - 1][];
            for (var i = 0; i < codeCoreLists.Count - 1; i++)
            {
                if (logWriter != null)
                {
                    logWriter.WriteLine($"Getting weights for row {i + 1}/{codeCoreLists.Count - 1}...");
                }
                res[i] = new double[codeCoreLists.Count - i + 1];
                for (var j = i + 1; j < codeCoreLists.Count; j++)
                {
                    var w = GetWeight(codeCoreLists[i], codeCoreLists[j], inputTol);
                    res[i][j - i - 1] = w;
                }
            }
            return res;
        }

        public static double[][] GetWeightsParallel(this IList<CodeCoreList> codeCoreLists, double inputTol, TextWriter logWriter = null)
        {
            var res = new double[codeCoreLists.Count - 1][];
            var startTime = DateTime.Now;
            var totalCoreCount = 0;
            var coresSoFar = 0;
            if (logWriter != null)
            {
                totalCoreCount = codeCoreLists.Sum(x => x.Cores.Count);
            }
            Parallel.For(0, codeCoreLists.Count-1, i =>
            {
                res[i] = new double[codeCoreLists.Count - i + 1];
                for (var j = i + 1; j < codeCoreLists.Count; j++)
                {
                    var w = GetWeight(codeCoreLists[i], codeCoreLists[j], inputTol);
                    res[i][j - i - 1] = w;
                }
                if (logWriter != null && codeCoreLists[i].Cores.Count > 0)
                {
                    lock (logWriter)
                    {
                        var c1 = codeCoreLists[i].Cores.Count;
                        coresSoFar += c1;
                        var time = DateTime.Now;
                        var elapsed = time - startTime;
                        var r = (double)coresSoFar/totalCoreCount;
                        var expected = TimeSpan.FromSeconds(elapsed.TotalSeconds / r);
                        logWriter.WriteLine($"Got weights {r*100:.00}% complete; time: {elapsed.TotalSeconds:0}/{expected.TotalSeconds-elapsed.TotalSeconds:0}/{expected.TotalSeconds:0} (secs)");
                    }
                }
            });
            return res;
        }

        /// <summary>
        ///  Returns the weight 
        /// </summary>
        /// <param name="ccl1"></param>
        /// <param name="ccl2"></param>
        /// <param name="inputTol"></param>
        /// <returns></returns>
        public static double GetWeight(CodeCoreList ccl1, CodeCoreList ccl2, double inputTol)
        {
            if (ccl2.Cores.Count < ccl1.Cores.Count)
            {
                return GetWeightSmallFirst(ccl2, ccl1, inputTol);
            }
            return GetWeightSmallFirst(ccl1, ccl2, inputTol);
        }

        public static double GetWeightParallel(CodeCoreList ccl1, CodeCoreList ccl2, double inputTol)
        {
            if (ccl2.Cores.Count < ccl1.Cores.Count)
            {
                return GetWeightSmallFirstParallel(ccl2, ccl1, inputTol);
            }
            return GetWeightSmallFirstParallel(ccl1, ccl2, inputTol);
        }

        public static double GetWeightSmallFirst(CodeCoreList ccl1, CodeCoreList ccl2, double inputTol)
        {
            if (ccl1.Cores.Count == 0)
            {
                return 0;
            }
            var inputLen = ccl1.Cores[0].Core.CentersInput.Count;
            var outputLen = ccl1.Cores[0].Core.CentersOutput.Count;
            var scoreSum = 0.0;
            var scoreCount = 0;
            foreach (var c in ccl1.Cores)
            {
                // find a match in the second code base don ndintol
                var c2s = Search(c, ccl2, inputTol);
                var countSubtotal = 0;
                foreach (var c2 in c2s)
                {
                    var din = c2.DistanceIndicator; // this shouldn't be normailsed with length
                    var dout = c.Core.GetOutputAbsDistance(c2.Core);
                    var ndin = din / inputLen;
                    var ndout = dout / outputLen;
                    var score = GetScore(ndin, ndout);
                    scoreSum += score;
                    countSubtotal++;
                }
                scoreCount += countSubtotal > 0? countSubtotal : 1; // at least increment it so the score won't be overly high
            }
            if (scoreSum < 0) scoreSum = 0;
            var scoreMean = scoreCount > 0 ? scoreSum / scoreCount : 0;
            return scoreMean;
        }


        /// <summary>
        ///  Get the weight (correlation) between two core lists
        /// </summary>
        /// <param name="ccl1"></param>
        /// <param name="ccl2"></param>
        /// <param name="inputTol"></param>
        /// <returns></returns>
        public static double GetWeightSmallFirstParallel(CodeCoreList ccl1, CodeCoreList ccl2, double inputTol)
        {
            if (ccl1.Cores.Count == 0)
            {
                return 0;
            }
            var inputLen = ccl1.Cores[0].Core.CentersInput.Count;
            var outputLen = ccl1.Cores[0].Core.CentersOutput.Count;
            var scoreSum = 0.0;
            var scoreCount = 0;
            Parallel.ForEach(ccl1.Cores, () => default(SubtotalObject), (c, loopState, local) =>
             {
                 // find a match in the second code base don ndintol
                 var c2s = Search(c, ccl2, inputTol);
                 double scoreSubtotal = 0;
                 int countSubtotal = 0;
                 foreach (var c2 in c2s)
                 {
                     var din = c2.DistanceIndicator; // this shouldn't be normailsed with length
                     var dout = c.Core.GetOutputAbsDistance(c2.Core);
                     var ndin = din / inputLen;
                     var ndout = dout / outputLen;
                     var score = GetScore(ndin, ndout);
                     scoreSubtotal += score;
                     countSubtotal++;
                 }
                 return new SubtotalObject { ScoreSubtotal = scoreSubtotal, CountSubtotal = countSubtotal };
             }, (x) =>
             {
                 lock (ccl1)
                 {
                     scoreSum += x.ScoreSubtotal;
                     scoreCount += x.CountSubtotal;
                 }
             });
            if (scoreSum < 0) scoreSum = 0;
            var scoreMean = scoreCount > 0 ? scoreSum / scoreCount : 0;
            return scoreMean;
        }

        private static double GetScore(double ndin, double ndout)
        {
            if (ndout <= ndin)
            {
                return 1;
            }
            if (ndout >= ndin * 2)
            {
                return -1;
            }
            return 3 - 2 * (ndout / ndin);
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="point"></param>
        /// <param name="ndin"></param>
        /// <param name="ndout"></param>
        /// <param name="ndintol"></param>
        /// <param name="ndouttol"></param>
        /// <param name="inremain"></param>
        /// <returns></returns>
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

        /// <summary>
        ///  Find matches of <paramref name="ce"/> in <paramref name="ccl"/> with square
        ///  distance within the tolerance of <paramref name="ndintol"/>
        /// </summary>
        /// <param name="ce">The core to find matches for</param>
        /// <param name="ccl">The corelist</param>
        /// <param name="inputTol">tolerance of input mean absolute difference</param>
        /// <returns>All cores in the core list that satisfies criteria</returns>
        public static IEnumerable<CoreExtensionAndDist> Search(CoreExtension ce, CodeCoreList ccl, double inputTol)
        {
            // get to the one in ccl.Cores that's most promising (potentially closest to ce)
            var index = ccl.Cores.BinarySearch(ce, CoreInputMeanComparer.Instance);
            if (index < 0)
            {
                index = -index-1;
            }

            var inputLen = ce.Core.CentersInput.Count;
            var inputTolSum = inputTol * inputLen;
            for (var i = Math.Min(index, ccl.Cores.Count - 1); i >= 0; i--)
            {
                var d = ce.Core.GetInputAbsDistance(ccl.Cores[i].Core);
                if (d <= inputTolSum)
                {
                    // it satisfies the requirement
                    yield return new CoreExtensionAndDist(ccl.Cores[i], d);
                }
                else
                {
                    // for square distance, it uses a1^2+a2^2+...+aN^2 >= (a1+a2+...+aN)^2 / N
                    // for absolute distance, it uses |a1|+|a2|+...+|aN| >= |a1+a2+...aN|
                    var ss = ce.Core.GetInputDifferenceAbs(ccl.Cores[i].Core);
                    if (ss > inputTolSum)
                    {
                        // no need to search, no more items beyond this can satisfy the requirement
                        break;
                    }
                }
            }

            for (var i = index + 1; i < ccl.Cores.Count; i++)
            {
                var d = ce.Core.GetInputAbsDistance(ccl.Cores[i].Core);
                if (d <= inputTolSum)
                {
                    // it satisfies the requirement
                    yield return new CoreExtensionAndDist(ccl.Cores[i], d);
                }
                else
                {
                    // this uses a1^2+a2^2+...+aN^2 >= (a1+a2+...+aN)^2 / N
                    var ss = ce.Core.GetInputDifferenceAbs(ccl.Cores[i].Core);
                    if (ss > inputTolSum)
                    {
                        // no need to search, no more items beyond this can satisfy the requirement
                        break;
                    }
                }
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
