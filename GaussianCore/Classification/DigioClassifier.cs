using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GaussianCore.Helpers;

namespace GaussianCore.Classification
{
    /// <summary>
    ///  Non-directional weight provider
    /// </summary>
    public class DigioClassifier
    {
        #region Nested types

        /// <summary>
        ///  A list of cores with the same code
        /// </summary>
        public class CodeCoreList : IComparable<CodeCoreList>
        {
            /// <summary>
            ///  The code the cores share
            /// </summary>
            public string Code { get; set; }

            /// <summary>
            ///  The list of cores
            /// </summary>
            public List<CoreExtension> Cores { get; } = new List<CoreExtension>();

            #region IComparable<CodeCoreList> members

            public int CompareTo(CodeCoreList other)
            {
                return string.Compare(Code, other.Code, StringComparison.Ordinal);
            }

            #endregion

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
            public double DistanceIndicator { get; }
        }

        private struct SubtotalObject
        {
            public double ScoreSubtotal { get; set; }
            public int CountSubtotal { get; set; }
        }


        #endregion

        #region Fields

        /// <summary>
        ///  returns the weight from the weight table. the weight table is organised as below
        ///     (0,1), (0,2), (0,3), (0,4), ...
        ///     (1,2), (1,3), (1,4), ...
        ///     (2,3), (2,4), ...
        ///     ...
        /// </summary>
        private readonly List<List<DigioClassifyInfo>> _weights = new List<List<DigioClassifyInfo>>();

        private List<string> _codes;
        private List<CodeCoreList> _codeLists;

        #endregion

        #region Properties

        public IList<CodeCoreList> CoreLists
        {
            get { return _codeLists; }
            private set
            {
                _codeLists = value as List<CodeCoreList>?? value.ToList();
                _codeLists.Sort();
                _codes = null;
            }
        }

        public List<string> Codes => _codes ?? (_codes = CoreLists?.Select(x => x.Code).ToList());

        public double InputTolerance { get; set; }

        public TextWriter Logger { get; set; }

        #endregion

        #region Methods

        public double GetWeight(int i, int j)
        {
            if (i < j)
            {
                return _weights[i][j - i - 1].Score;
            }
            if (i > j)
            {
                return _weights[j][i - j - 1].Score;
            }
            return 1;
        }

        public void ReCreate(List<CodeCoreList> codeCoreLists)
        {
            CoreLists = codeCoreLists;
            _weights.ReAlloc(codeCoreLists.Count - 1);
            for (var i = 0; i < codeCoreLists.Count - 1; i++)
            {
                Logger?.WriteLine($"Getting weights for row {i + 1}/{codeCoreLists.Count - 1}...");
                var row = new List<DigioClassifyInfo>(codeCoreLists.Count - i + 1);
                for (var j = i + 1; j < codeCoreLists.Count; j++)
                {
                    var w = GetWeight(codeCoreLists[i], codeCoreLists[j], InputTolerance);
                    row.Add(w);
                }
                _weights.Add(row);
            }
        }

        public void ReCreateParallel(IList<CodeCoreList> codeCoreLists)
        {
            CoreLists = codeCoreLists;
            _weights.ReAllocAndInit(codeCoreLists.Count - 1);
            var startTime = DateTime.Now;
            var totalCoreCount = 0;
            var coresSoFar = 0;
            if (Logger != null)
            {
                totalCoreCount = codeCoreLists.Sum(x => x.Cores.Count);
            }
            Parallel.For(0, codeCoreLists.Count - 1, i =>
            {
                _weights[i] = new List<DigioClassifyInfo>(codeCoreLists.Count - i + 1);
                for (var j = i + 1; j < codeCoreLists.Count; j++)
                {
                    var w = GetWeight(codeCoreLists[i], codeCoreLists[j], InputTolerance);
                    _weights[i].Add(w);
                }
                if (Logger != null && codeCoreLists[i].Cores.Count > 0)
                {
                    lock (Logger)
                    {
                        var c1 = codeCoreLists[i].Cores.Count;
                        coresSoFar += c1;
                        var time = DateTime.Now;
                        var elapsed = time - startTime;
                        var r = (double)coresSoFar / totalCoreCount;
                        var expected = TimeSpan.FromSeconds(elapsed.TotalSeconds / r);
                        Logger.WriteLine($"Got weights {r * 100:.00}% complete; time: {elapsed.TotalSeconds:0}/{expected.TotalSeconds - elapsed.TotalSeconds:0}/{expected.TotalSeconds:0} (secs)");
                    }
                }
            });
        }
        
        public void Update(IList<CodeCoreList> corelists, IList<int> toAdd)
        {
            CoreLists = corelists;
            if (toAdd == null)
            {
                toAdd = new int[] {};
            }
            if (toAdd.Count > 0)
            {
                var newCount = _weights.Count + toAdd.Count;
                // rows
                foreach (var i in toAdd)
                {
                    var items = new List<DigioClassifyInfo>();
                    for (var j = i + 1; j < newCount; j++)
                    {
                        var w = GetWeight(corelists[i], corelists[j], InputTolerance);
                        items.Add(w);
                    }
                    _weights.Insert(i, items);
                }
                // TODO test this...
                // colums
                var ki = 0;
                for (var i = 0; i < _weights.Count; i++)
                {
                    var row = _weights[i];
                    var k = ki;
                    for (; toAdd[k] < i + 1; k++)
                    {
                        // empty loop, nothing to be done here
                    }
                    ki = k;
                    for (var j = i + 1; j < _weights.Count+1; j++)
                    {
                        if (toAdd[k] == j)
                        {
                            var w = GetWeight(corelists[i], corelists[j], InputTolerance);
                            row.Insert(j - i - 1, w);
                            k++;
                        }
                    }
                }
            }
           
            // TODO existing ones
            var k1 = 0;
            for (var i = 0; i < _weights.Count; i++)
            {
                if (k1 < toAdd.Count && i == toAdd[k1])
                {
                    k1++;
                    continue;
                }
                var k2 = k1;
                for (var j = i + 1; j < _weights.Count + 1; j++)
                {
                    if (k2 < toAdd.Count && j == toAdd[k2])
                    {
                        k2++;
                        continue;
                    }
                    Update(_weights[i][j - i - 1], corelists[i], corelists[j]);
                }
            }

        }

        private void Update(DigioClassifyInfo dci, CodeCoreList ccl1, CodeCoreList ccl2)
        {
            if (ccl2.Cores.Count < ccl1.Cores.Count)
            {
                UpdateWeightSmallFirst(dci, ccl2, ccl1, InputTolerance);
            }
            else
            {
                UpdateWeightSmallFirst(dci, ccl1, ccl2, InputTolerance);
            }
        }

        public void SetCoreLists(IList<CodeCoreList> ccl)
        {
            CoreLists = ccl;
        }

        public void Save(string savePath)
        {
            using (var f = new FileStream(savePath, FileMode.Create))
            using (var bw = new BinaryWriter(f))
            {
                bw.Write(Codes.Count);
                foreach (var t in Codes)
                {
                    bw.Write(t);
                }
                foreach (var t in _weights.SelectMany(row => row))
                {
                    t.WriteToBinary(bw);
                }
            }
        }

        public void Load(string loadPath)
        {
            using (var f = new FileStream(loadPath, FileMode.Open))
            using (var br = new BinaryReader(f))
            {
                var len = br.ReadInt32();
                _weights.ReAllocAndInit(len-1);
                for (var i = 0; i < len - 1; i++)
                {
                    _weights[i] = new List<DigioClassifyInfo>(len - i - 1);
                    for (var j = 0; j < len - i - 1; j++)
                    {
                        _weights[i].Add(DigioClassifyInfo.ReadFromBinary(br));
                    }
                }
            }
        }

        public static void UpdateWeightSmallFirst(DigioClassifyInfo dci, CodeCoreList ccl1, CodeCoreList ccl2,
            double inputTol)
        {
            if (dci.EndOfOne == ccl1.Cores.Count && dci.EndOfTwo == ccl2.Cores.Count)
            {
                // no need to update
                return;
            }
            var inputLen = ccl1.Cores[0].Core.CentersInput.Count;
            var outputLen = ccl1.Cores[0].Core.CentersOutput.Count;
            // TODO UPDATE...
            var scoreSum = 0.0;
            var scoreCount = 0;
            for (var i = 0; i < ccl1.Cores.Count; i++)
            {
                var c = ccl1.Cores[i];
                var ccl2SearchRange = (i < dci.EndOfOne)
                    ? ccl2.Cores.GetRange(dci.EndOfTwo, ccl2.Cores.Count - dci.EndOfTwo)
                    : ccl2.Cores;
                var cs2 = Search(c, ccl2SearchRange, inputTol);
                var countSubtotal = 0;
                foreach (var c2 in cs2)
                {
                    var din = c2.DistanceIndicator; // this shouldn't be normailsed with length
                    var dout = c.Core.GetOutputAbsDistance(c2.Core);
                    var ndin = din / inputLen;
                    var ndout = dout / outputLen;
                    var score = GetScore(ndin, ndout);
                    scoreSum += score;
                    countSubtotal++;
                }
                scoreCount += countSubtotal > 0 ? countSubtotal : 1; // at least increment it so the score won't be overly high
            }
            var newScoreSum = dci.Score*dci.Count;// old sum
            dci.Count += scoreCount;
            newScoreSum += scoreSum;
            dci.Score = newScoreSum/ dci.Count;
            dci.EndOfTwo = ccl1.Cores.Count;
            dci.EndOfOne = ccl2.Cores.Count;
        }

        /// <summary>
        ///  Returns the weight 
        /// </summary>
        /// <param name="ccl1"></param>
        /// <param name="ccl2"></param>
        /// <param name="inputTol"></param>
        /// <returns></returns>
        public static DigioClassifyInfo GetWeight(CodeCoreList ccl1, CodeCoreList ccl2, double inputTol)
        {
            if (ccl2.Cores.Count < ccl1.Cores.Count)
            {
                return GetWeightSmallFirst(ccl2, ccl1, inputTol);
            }
            return GetWeightSmallFirst(ccl1, ccl2, inputTol);
        }

        public static DigioClassifyInfo GetWeightParallel(CodeCoreList ccl1, CodeCoreList ccl2, double inputTol)
        {
            if (ccl2.Cores.Count < ccl1.Cores.Count)
            {
                return GetWeightSmallFirstParallel(ccl2, ccl1, inputTol);
            }
            return GetWeightSmallFirstParallel(ccl1, ccl2, inputTol);
        }

        public static DigioClassifyInfo GetWeightSmallFirst(CodeCoreList ccl1, CodeCoreList ccl2, double inputTol)
        {
            if (ccl1.Cores.Count == 0)
            {
                return DigioClassifyInfo.Default;
            }
            var inputLen = ccl1.Cores[0].Core.CentersInput.Count;
            var outputLen = ccl1.Cores[0].Core.CentersOutput.Count;
            var scoreSum = 0.0;
            var scoreCount = 0;
            foreach (var c in ccl1.Cores)
            {
                // find a match in the second code base don ndintol
                var cs2 = Search(c, ccl2.Cores, inputTol);
                var countSubtotal = 0;
                foreach (var c2 in cs2)
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
            return new DigioClassifyInfo(scoreMean, scoreCount, ccl1.Cores.Count, ccl2.Cores.Count);
        }

        /// <summary>
        ///  Get the weight (correlation) between two core lists
        /// </summary>
        /// <param name="ccl1"></param>
        /// <param name="ccl2"></param>
        /// <param name="inputTol"></param>
        /// <returns></returns>
        public static DigioClassifyInfo GetWeightSmallFirstParallel(CodeCoreList ccl1, CodeCoreList ccl2, double inputTol)
        {
            if (ccl1.Cores.Count == 0)
            {
                return DigioClassifyInfo.Default;
            }
            var inputLen = ccl1.Cores[0].Core.CentersInput.Count;
            var outputLen = ccl1.Cores[0].Core.CentersOutput.Count;
            var scoreSum = 0.0;
            var scoreCount = 0;
            Parallel.ForEach(ccl1.Cores, () => default(SubtotalObject), (c, loopState, local) =>
            {
                // find a match in the second code base don ndintol
                var cs2 = Search(c, ccl2.Cores, inputTol);
                double scoreSubtotal = 0;
                int countSubtotal = 0;
                foreach (var c2 in cs2)
                {
                    var din = c2.DistanceIndicator; // this shouldn't be normailsed with length
                    var dout = c.Core.GetOutputAbsDistance(c2.Core);
                    var ndin = din/inputLen;
                    var ndout = dout/outputLen;
                    var score = GetScore(ndin, ndout);
                    scoreSubtotal += score;
                    countSubtotal++;
                }
                return new SubtotalObject {ScoreSubtotal = scoreSubtotal, CountSubtotal = countSubtotal};
            }, x =>
            {
                lock (ccl1)
                {
                    // ReSharper disable once AccessToModifiedClosure
                    scoreSum += x.ScoreSubtotal;
                    scoreCount += x.CountSubtotal;
                }
            });
            if (scoreSum < 0) scoreSum = 0;
            var scoreMean = scoreCount > 0 ? scoreSum / scoreCount : 0;
            return new DigioClassifyInfo(scoreMean, scoreCount, ccl1.Cores.Count, ccl2.Cores.Count);
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
        ///  Find matches of <paramref name="ce"/> in <paramref name="ccl"/> with square
        ///  distance within the tolerance of <paramref name="inputTol"/>
        /// </summary>
        /// <param name="ce">The core to find matches for</param>
        /// <param name="ccl">The corelist</param>
        /// <param name="inputTol">tolerance of input mean absolute difference</param>
        /// <returns>All cores in the core list that satisfies criteria</returns>
        public static IEnumerable<CoreExtensionAndDist> Search(CoreExtension ce, List<CoreExtension> ccl, double inputTol)
        {
            // get to the one in ccl.Cores that's most promising (potentially closest to ce)
            var index = ccl.BinarySearch(ce, CoreInputMeanComparer.Instance);
            if (index < 0)
            {
                index = -index-1;
            }

            var inputLen = ce.Core.CentersInput.Count;
            var inputTolSum = inputTol * inputLen;
            for (var i = Math.Min(index, ccl.Count - 1); i >= 0; i--)
            {
                var d = ce.Core.GetInputAbsDistance(ccl[i].Core);
                if (d <= inputTolSum)
                {
                    // it satisfies the requirement
                    yield return new CoreExtensionAndDist(ccl[i], d);
                }
                else
                {
                    // for square distance, it uses a1^2+a2^2+...+aN^2 >= (a1+a2+...+aN)^2 / N
                    // for absolute distance, it uses |a1|+|a2|+...+|aN| >= |a1+a2+...aN|
                    var ss = ce.Core.GetInputDifferenceAbs(ccl[i].Core);
                    if (ss > inputTolSum)
                    {
                        // no need to search, no more items beyond this can satisfy the requirement
                        break;
                    }
                }
            }

            for (var i = index + 1; i < ccl.Count; i++)
            {
                var d = ce.Core.GetInputAbsDistance(ccl[i].Core);
                if (d <= inputTolSum)
                {
                    // it satisfies the requirement
                    yield return new CoreExtensionAndDist(ccl[i], d);
                }
                else
                {
                    // this uses a1^2+a2^2+...+aN^2 >= (a1+a2+...+aN)^2 / N
                    var ss = ce.Core.GetInputDifferenceAbs(ccl[i].Core);
                    if (ss > inputTolSum)
                    {
                        // no need to search, no more items beyond this can satisfy the requirement
                        break;
                    }
                }
            }
        }

        #endregion
    }
}
