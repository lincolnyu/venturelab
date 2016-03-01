using System.IO;
using System.Linq;
using GaussianCore.Classification;
using static GaussianCore.Classification.DigioClassifier;
using System.Collections.Generic;

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
        /// <param name="weights">The weight table loaded</param>
        /// <param name="codes">All the loaded codes corresponding to the weight table</param>
        /// <param name="logWriter">stream to write logs (optional)</param>
        public static void GetWeights(string statisticsDir, double inputTol, out double[][] weights, out string[] codes,
            bool parallel = false, TextWriter logWriter = null)
        {
            var dir = new DirectoryInfo(statisticsDir);
            var files = dir.GetFiles();
            var corelists = new List<CodeCoreList>();
            var lengths = ExtractHelper.GetLengths(statisticsDir);
            // NOTE somehow we can't parallelise this...
            foreach (var f in files)
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
                            var cores = ExtractHelper.LoadCores(br, count, 0, 5);
                            var corelist = new CodeCoreList();
                            corelist.Cores.AddRange(cores.Select(x => new CoreExtension { Core = x }));
                            corelist.Cores.ForEach(x => x.Update());
                            corelist.Code = code;
                            corelist.SortCores();
                            corelists.Add(corelist);
                        }
                        else if (logWriter != null)
                        {
                            logWriter.WriteLine($"{code} not found!!!");
                        }
                    }
                }
            }
            logWriter?.WriteLine("Computing weights");
            weights = parallel ? corelists.GetWeightsParallel(inputTol, logWriter)
                : corelists.GetWeights(inputTol, logWriter);
            codes = corelists.Select(x => x.Code).ToArray();
        }
        
        public static void PrintWeightsToCsv(string statisticsDir, string csv, double inputTol, bool parallel = false, TextWriter logWriter = null)
        {
            double[][] weights;
            string[] codes;
            GetWeights(statisticsDir, inputTol, out weights, out codes, parallel, logWriter);
            using (var sw = new StreamWriter(csv))
            {
                // header
                sw.Write(",");
                for (var i = 0; i < codes.Length; i++)
                {
                    sw.Write("{0},", codes[i]);
                }
                sw.WriteLine();
                for (var i = 0; i < codes.Length; i++)
                {
                    sw.Write("{0},", codes[i]);
                    for (var j = 0; j < codes.Length; j++)
                    {
                        if (i == j)
                        {
                            sw.Write("/,");
                        }
                        else if (i < j)
                        {
                            sw.Write("{0},", weights[i][j-i-1]);
                        }
                        else
                        {
                            sw.Write("{0},", weights[j][i-j-1]);
                        }
                    }
                    sw.WriteLine();
                }
            }
        }

        #endregion
    }
}
