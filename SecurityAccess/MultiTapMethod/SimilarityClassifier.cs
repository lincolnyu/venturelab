using GaussianCore;
using GaussianCore.Generic;
using GaussianCore.Classification;
using SecurityAccess.Asx;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SecurityAccess.MultiTapMethod
{
    public class SimilarityClassifier
    {
        #region Nested types

        public class Similar
        {
            public FixedConfinedCoreManager CoreManager { get; private set; } = new FixedConfinedCoreManager();

            public List<string> Codes { get; private set; } = new List<string>();
        }

        private struct CoreSet
        {
            public string Code { get; set; }

            public List<ICore> Cores { get; set; }
        }

        #endregion

        #region Constructors

        public SimilarityClassifier(string dir)
        {
            StatisticsDir = dir;
        }

        #endregion

        #region Properties

        public string StatisticsDir { get; private set; }

        public ISet<Similar> Similars { get; private set; } = new HashSet<Similar>();

        #endregion

        #region Methods

        public void Classify()
        {
            Similars.Clear();
            var coresets = GetCoreSets();
            var sdtable = GetSquareDistanceTable(coresets);
        }

        private List<CoreSet> GetCoreSets()
        {
            var dir = new DirectoryInfo(StatisticsDir);
            var files = dir.GetFiles();

            var coresets = new List<CoreSet>();
            foreach (var file in files.Where(f => f.Extension.ToLower().Equals(".dat")))
            {
                string code;
                if (!FileReorganiser.IsCodeFile(file.Name, out code))
                {
                    continue;
                }
                using (var fs = file.OpenRead())
                {
                    using (var sr = new StreamReader(fs))
                    {
                        var cores = FixedConfinedBuilder.GetCores(sr);
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

        private static double[][] GetSquareDistanceTable(List<CoreSet> coresets)
        {
            var table = new double[coresets.Count - 1][];
            for (var i = 0; i < coresets.Count - 1; i++)
            {
                table[i] = new double[coresets.Count - i - 1];
                var cs1 = coresets[i].Cores;
                for (var j = i + 1; j < coresets.Count; j++)
                {
                    var cs2 = coresets[j].Cores;
                    var sd = cs1.GetSquareDistance(cs2);
                    table[i][j - i - 1] = sd;
                }
            }
            return table;
        }

        #endregion
    }
}
