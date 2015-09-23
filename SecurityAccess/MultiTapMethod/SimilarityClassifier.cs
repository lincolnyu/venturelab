using GaussianCore;
using GaussianCore.Classification;
using SecurityAccess.Asx;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using GaussianCore.Generic;
using QLogger;

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

            public double Distance { get; set; }

            public int CompareTo(DistanceEntry other)
            {
                return Distance.CompareTo(other.Distance);
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

        public PerformanceLogger Logger { get; private set; } = new PerformanceLogger();

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
            foreach (var file in files.Where(f => f.Extension.ToLower().Equals(".dat")).OrderBy(x=>x.Name))
            {
                string code, dummy;
                if (!FileReorganiser.IsCodeFile(file.Name, out code, out dummy))
                {
                    continue;
                }
                Logger.Write("Loading code {0}...", code);
                using (var fs = file.OpenRead())
                {
                    using (var br = new BinaryReader(fs))
                    {
                        int count;
                        FixedConfinedBuilder.Flags flag;
                        FixedConfinedBuilder.GetHeader(br, out count, out flag);
                        if (count > 0)
                        {
                            var cores = FixedConfinedBuilder.LoadCores(br, count, flag);
                            var list = cores.ToList();
                            var coreset = new CoreSet
                            {
                                Code = code,
                                Cores = list
                            };
                            coresets.Add(coreset);
                            Logger.WriteLine("done ({0} cores loaded)", count);
                        }
                        else
                        {
                            Logger.WriteLine("ignored.");
                        }
                    }
                }
            }
            return coresets;
        }

        public List<DistanceEntry> GetOrderedSquareDistances(List<CoreSet> coresets)
        {
            var list = new List<DistanceEntry>();

            Logger.WriteLine("Analyses distances...");

            var outb = Logger.InplaceWrite(0, "Getting distance between ");
            var b = 0;
            for (var i = 0; i < coresets.Count - 1; i++)
            {
                var cs1 = coresets[i].Cores;
                for (var j = i + 1; j < coresets.Count; j++)
                {
                    b = Logger.InplaceWrite(b, "{0} and {1}...", i, j);

                    var cs2 = coresets[j].Cores;
                    var sd = cs1.GetQuanbenDistance(cs2);
                    list.Add(new DistanceEntry
                    {
                        CoreSet1 = coresets[i],
                        CoreSet2 = coresets[j],
                        Distance = sd
                    });
                }
            }
            list.Sort();

            Logger.InplaceWriteLine(outb + b, "Distance analysis done");

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
            minDistance = Math.Sqrt(dists.Select(x => x.Distance).Min());
            maxDistance = Math.Sqrt(dists.Select(x => x.Distance).Max());
            meanDistance = dists.Select(x => Math.Sqrt(x.Distance)).Average();
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

            Logger.WriteLine("Classifying...");

            Classify(coresets, orderedDistances, quit, similars, codeToSimilar);

            Logger.WriteLine("Classification done.");

            var descFilePath = Path.Combine(similarsDir, "_info.txt");

            using (var descf = new StreamWriter(descFilePath))
            {
                var i = 1;
                foreach (var sim in similars)
                {
                    Logger.WriteLine("Processing similar set {0} (with {1} coresets)...", i, sim.CoreSets.Count);
                    var fn = string.Format("{0}.dat", i);
                    var path = Path.Combine(similarsDir, fn);
                    var fccm = new FixedConfinedCoreManager();
                    var builder = new FixedConfinedBuilder(fccm);
                    var b = 0;
                    foreach (var cs in sim.CoreSets)
                    {
                        b = Logger.InplaceWrite(b, "Processing coreset {0} (with {1} cores)", cs.Code, cs.Cores.Count);
                        builder.BuildFromCoreSet(cs.Cores, true, false);
                        // <code>:<similar set index>:<num of cores the code contributes>
                        descf.WriteLine("{0}:{1}:{2}", cs.Code, i, cs.Cores.Count);
                    }
                    Logger.InplaceWriteLine(b, "done");
                    Logger.WriteStart("Updating core coeffs ({0} cores)...", fccm.Cores.Count);
                    fccm.UpdateCoreCoeffs();
                    Logger.WriteLineEnd("done.");
                    Logger.WriteStart("Saving similar set {0}...", i);
                    builder.Save(path, flag);
                    Logger.WriteLineEnd("done.");
                    i++;
                }
            }
        }

        public void Classify(List<CoreSet> coresets,
            IList<DistanceEntry> orderedDistances, Predicate<DistanceEntry> quit,
            ISet<Similar> similars, IDictionary<string, Similar> codeToSimilar, 
            TextWriter logger = null)
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
