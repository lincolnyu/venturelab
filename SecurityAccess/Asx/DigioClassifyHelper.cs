using System.IO;
using System.Linq;
using GaussianCore.Classification;
using static GaussianCore.Classification.DigioClassifier;
using System.Collections.Generic;
using SecurityAccess.MultiTapMethod;

namespace SecurityAccess.Asx
{
    public static class DigioClassifyHelper
    {
        #region Methods

        /// <summary>
        ///  Gets weights
        /// </summary>
        /// <param name="statisticsDir">The directory that contains binary statistical (quanbenMetrics) files one per code</param>
        /// <param name="inputTol"></param>
        /// <param name="start"></param>
        /// <param name="step"></param>
        /// <param name="parallel">whether to get the weights </param>
        /// <param name="logWriter">stream to write logs (optional)</param>
        public static DigioClassifier GetNewDigioClassifier(string statisticsDir, 
            double inputTol, int start = 0, int step = 1, bool parallel = false, 
            TextWriter logWriter = null)
        {
            var corelists = GetCurrentStatistics(statisticsDir, start, step);
            logWriter?.WriteLine("Computing weights");
            var dci = new DigioClassifier
            {
                InputTolerance = inputTol,
                Logger = logWriter
            };
            if (parallel)
            {
                dci.ReCreateParallel(corelists);
            }
            else
            {
                dci.ReCreate(corelists);
            }
            return dci;
        }

        public static void GetNewDigioClassifierAndSave(string statisticsDir, string savePath,
            double inputTol, int start = 0, int step = 1,
            bool parallel = false, TextWriter logWriter = null)
        {
            var dc = GetNewDigioClassifier(statisticsDir, inputTol, start, step, parallel, logWriter);
            dc.Save(savePath);
        }

        public static void UpdateWeights(DigioClassifier dc, string statisticsDir,
            double inputTol, int start = 0, int step = 1, bool parallel = false,
            TextWriter logWriter = null)
        {
            var corelists = GetCurrentStatistics(statisticsDir, start, step, logWriter);
            var a = 0;
            var toInsert = new List<int>();
            var codes = dc.Codes?.ToList() ?? new List<string>();
            for (var i = 0; i < corelists.Count; i++)
            {
                var code = corelists[i].Code;
                if (code == codes[a])
                {
                    a++;
                }
                else
                {
                    codes.Insert(a, code);
                    toInsert.Add(i);
                }
            }
            dc.Update(corelists, toInsert);
        }

        private static List<CodeCoreList> GetCurrentStatistics(string statisticsDir,
            int start = 0, int step = 1, TextWriter logWriter = null)
        {
            var dir = new DirectoryInfo(statisticsDir);
            var files = dir.GetFiles();
            var corelists = new List<CodeCoreList>();
            var lengths = ExtractHelper.GetLengths(statisticsDir);
            // NOTE somehow we can't parallelise this...
            foreach (var f in files.OrderBy(x=>x.Name))
            {
                string code, ext;
                if (!(FileReorganiser.IsCodeFile(f.Name, out code, out ext) && ext == ".dat"))
                {
                    continue;
                }
                using (var fs = f.Open(FileMode.Open))
                {
                    using (var br = new BinaryReader(fs))
                    {
                        int count;
                        if (lengths.TryGetValue(code, out count))
                        {
                            var cores = ExtractHelper.LoadCores(br, count, start, step);
                            var corelist = new CodeCoreList();
                            corelist.Cores.AddRange(cores.Select(x => new CoreExtension { Core = x }));
                            corelist.Cores.ForEach(x => x.Update());
                            corelist.Code = code;
                            corelist.SortCores();
                            corelists.Add(corelist);
                        }
                        else
                        {
                            logWriter?.WriteLine($"{code} not found!!!");
                        }
                    }
                }
            }
            return corelists;
        }


        public static void LoadWeights(string loadPath, out DigioClassifyInfo[][] weights, out string[] codes)
        {
            using (var f = new FileStream(loadPath, FileMode.Open))
            using (var br = new BinaryReader(f))
            {
                var len = br.ReadInt32();
                codes = new string[len];
                for (var i = 0; i < len; i++)
                {
                    codes[i] = br.ReadString();
                }
                weights = new DigioClassifyInfo[len-1][];
                for (var i = 0; i < len-1; i++)
                {
                    weights[i] = new DigioClassifyInfo[len - i - 1];
                    for (var j = 0; j < len - i - 1; j++)
                    {
                        weights[i][j] = DigioClassifyInfo.ReadFromBinary(br);
                    }

                }
            }
        }

        public static void PrintWeightsToCsv(this DigioClassifier dc, string csv)
        {
            using (var sw = new StreamWriter(csv))
            {
                // header
                sw.Write(",");
                for (var i = 0; i < dc.Codes.Count; i++)
                {
                    sw.Write("{0},", dc.Codes[i]);
                }
                sw.WriteLine();
                for (var i = 0; i < dc.Codes.Count; i++)
                {
                    sw.Write("{0},", dc.Codes[i]);
                    for (var j = 0; j < dc.Codes.Count; j++)
                    {
                        if (i == j)
                        {
                            sw.Write("/,");
                        }
                        else
                        {
                            var w = dc.GetWeight(i, j);
                            sw.Write($"{w}");  
                        }
                    }
                    sw.WriteLine();
                }
            }
        }

        #endregion
    }
}
