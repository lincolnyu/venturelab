using GaussianCore;
using GaussianCore.Classification;
using SecurityAccess.Asx;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using GaussianCore.Generic;

namespace SecurityAccess.MultiTapMethod
{
    public class SimilarityClassifier
    {
        #region Nested types

        public class Similar
        {
            public ISet<CoreSet> CoreSets { get; private set; } = new HashSet<CoreSet>();

            public void Merge(Similar s1, Similar s2)
            {
                CoreSets.Clear();
                foreach (var cs in s1.CoreSets)
                {
                    CoreSets.Add(cs);
                }
                foreach (var cs in s2.CoreSets)
                {
                    CoreSets.Add(cs);
                }
            }
        }

        public struct CoreSet
        {
            public string Code { get; set; }

            public List<ICore> Cores { get; set; }
        }

        public class DistanceEntry : IComparable<DistanceEntry>
        { 
            public CoreSet CoreSet1 { get; set; }

            public CoreSet CoreSet2 { get; set; }

            public double SquareDistance { get; set; }

            public int CompareTo(DistanceEntry other)
            {
                return SquareDistance.CompareTo(other.SquareDistance);
            }
        }

        public struct CodeMapInfo
        {
            public int Index { get; set; }
            public int Count { get; set; }
        }

        #endregion

        #region Constructors

        public SimilarityClassifier(string statisticsDir)
        {
            StatisticsDir = statisticsDir;
        }

        #endregion

        #region Properties

        public string StatisticsDir { get; private set; }

        public ISet<Similar> Similars { get; private set; } = new HashSet<Similar>();

        #endregion

        #region Methods

        /// <summary>
        ///   retrieves all coresets from the binary statistical data file
        /// </summary>
        /// <returns></returns>
        public List<CoreSet> GetCoreSets()
        {
            var dir = new DirectoryInfo(StatisticsDir);
            var files = dir.GetFiles();

            var coresets = new List<CoreSet>();
            foreach (var file in files.Where(f => f.Extension.ToLower().Equals(".dat")))
            {
                string code, dummy;
                if (!FileReorganiser.IsCodeFile(file.Name, out code, out dummy))
                {
                    continue;
                }
                using (var fs = file.OpenRead())
                {
                    using (var br = new BinaryReader(fs))
                    {
                        int count;
                        FixedConfinedBuilder.Flags flag;
                        FixedConfinedBuilder.GetHeader(br, out count, out flag);
                        var cores = FixedConfinedBuilder.LoadCores(br, count, flag);
                        var list = cores.ToList();
                        var coreset = new CoreSet
                        {
                            Code = code,
                            Cores = list
                        };
                        coresets.Add(coreset);
                    }
                }
            }
            return coresets;
        }

        public static List<DistanceEntry> GetOrderedSquareDistances(List<CoreSet> coresets)
        {
            var list = new List<DistanceEntry>();
            for (var i = 0; i < coresets.Count - 1; i++)
            {
                var cs1 = coresets[i].Cores;
                for (var j = i + 1; j < coresets.Count; j++)
                {
                    var cs2 = coresets[j].Cores;
                    var sd = cs1.GetSquareDistance(cs2);
                    list.Add(new DistanceEntry
                    {
                        CoreSet1 = coresets[i],
                        CoreSet2 = coresets[j],
                        SquareDistance = sd
                    });
                }
            }
            list.Sort();
            return list;
        }

        /// <summary>
        ///  Returns several characteristic (not square) distance values
        /// </summary>
        /// <param name="dists">The distance list</param>
        /// <param name="meanDistance">The mean distance</param>
        /// <param name="minDistance">The minimum distance</param>
        /// <param name="maxDistance">The maximum distance</param>
        public void GetDistanceMetrics(IList<DistanceEntry> dists, out double meanDistance,
            out double minDistance, out double maxDistance)
        {
            minDistance = Math.Sqrt(dists.Select(x => x.SquareDistance).Min());
            maxDistance = Math.Sqrt(dists.Select(x => x.SquareDistance).Max());
            meanDistance = dists.Select(x => Math.Sqrt(x.SquareDistance)).Average();
        }

        /// <summary>
        ///  This only supports incomplete statistic entry mode
        /// </summary>
        /// <param name="similarsDir"></param>
        public void ClassifyInc(string similarsDir)
        {
            var codeToInfo = new Dictionary<string, CodeMapInfo>();
            var infoFilePath = Path.Combine(similarsDir, "_info.txt");
            GetCodeToInfo(infoFilePath, codeToInfo);

            foreach (var cip in codeToInfo)
            {
                var code = cip.Key;
                var index = cip.Value.Index;
                var count = cip.Value.Count;
                // TODO optimise by ignoring unchanged files
                var fn = string.Format("{0}.dat", index);
                var path = Path.Combine(similarsDir, fn);

                var srcFn = string.Format("{0}.dat", code);
                var srcPath = Path.Combine(StatisticsDir, fn);

                IEnumerable<ICore> newCores;
                using (var fs = new FileStream(srcPath, FileMode.Open))
                {
                    using (var br = new BinaryReader(fs))
                    {
                        FixedConfinedBuilder.Flags flag;
                        int newCount;
                        FixedConfinedBuilder.GetHeader(br, out newCount, out flag);
                        if (flag != FixedConfinedBuilder.Flags.InputOutputOnly)
                        {
                            throw new NotSupportedException("Inc classicifcation only works with incomplete statistics data");
                        }
                        newCores = FixedConfinedBuilder.LoadCores(br, count, newCount - count, flag);
                    }
                }

                var existingCount = codeToInfo[code].Count;
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    using (var bw = new BinaryWriter(fs))
                    {
                        FixedConfinedBuilder.SaveCores(bw, newCores, existingCount, FixedConfinedBuilder.Flags.InputOutputOnly);
                    }
                }
            }
        }

        public void Classify(List<CoreSet> coresets, IList<DistanceEntry> orderedDistances,
            Predicate<DistanceEntry> quit, string similarsDir, FixedConfinedBuilder.Flags flag
             = FixedConfinedBuilder.Flags.InputOutputOnly)
        {
            var similars = new HashSet<Similar>();
            var codeToSimilar = new Dictionary<string, Similar>();
            Classify(coresets, orderedDistances, quit, similars, codeToSimilar);

            var descFilePath = Path.Combine(similarsDir, "_info.txt");

            using (var descf = new StreamWriter(descFilePath))
            {
                var i = 1;
                foreach (var sim in similars)
                {
                    var fn = string.Format("{0}.dat", i);
                    var path = Path.Combine(similarsDir, fn);
                    var fccm = new FixedConfinedCoreManager();
                    var builder = new FixedConfinedBuilder(fccm);
                    foreach (var cs in sim.CoreSets)
                    {
                        builder.BuildFromCoreSet(cs.Cores, true, false);
                        // <code>:<similar set index>:<num of cores the code contributes>
                        descf.WriteLine("{0}:{1}:{2}", cs.Code, i, cs.Cores.Count);
                    }
                    fccm.UpdateCoreCoeffs();
                    builder.Save(path, flag);
                    i++;
                }
            }
        }

        public void Classify(List<CoreSet> coresets,
            IList<DistanceEntry> orderedDistances, Predicate<DistanceEntry> quit,
            ISet<Similar> similars, IDictionary<string, Similar> codeToSimilar)
        {
            // init similars
            similars.Clear();
            codeToSimilar.Clear();

            foreach (var cs in coresets)
            {
                var a = new Similar();
                a.CoreSets.Add(cs);
                codeToSimilar[cs.Code] = a;
                similars.Add(a);
            }

            foreach (var od in orderedDistances)
            {
                if (quit(od))
                {
                    break;
                }

                var s1 = od.CoreSet1.Code;
                var s2 = od.CoreSet2.Code;

                var sim1 = codeToSimilar[s1];
                var sim2 = codeToSimilar[s2];

                if (sim1 == sim2)
                {
                    continue;
                }

                var ns = new Similar();
                // merge
                ns.Merge(sim1, sim2);
                foreach (var cs in ns.CoreSets)
                {
                    codeToSimilar[cs.Code] = ns;
                }
                similars.Remove(sim1);
                similars.Remove(sim2);
                similars.Add(ns);
            }
        }

        private void GetCodeToInfo(string infoFilePath, IDictionary<string, CodeMapInfo> codeToInfo)
        {
            using (var sr = new StreamReader(infoFilePath))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (line == null) continue;
                    var split = line.Split(':');
                    var code = split[0];
                    var index = int.Parse(split[1]);
                    var count = int.Parse(split[2]);
                    codeToInfo[code] = new CodeMapInfo
                    {
                        Index = index,
                        Count = count
                    };
                }
            }
        }

        #endregion
    }
}
