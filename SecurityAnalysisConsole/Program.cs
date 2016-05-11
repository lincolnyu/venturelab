using System;
using SecurityAccess.Asx;
using SecurityAccess.MultiTapMethod;
using GaussianCore.Generic;
using System.IO;
using System.Linq;
using GaussianCore.Classification;

namespace SecurityAnalysisConsole
{
    class Program
    {
        static void ReorganiseFiles(string byDateDir, string byCodeDir, bool append = false)
        {
            var fr = new FileReorganiser(byDateDir, byCodeDir, Console.Out, append);
            fr.Reorganise();
        }

        static void SuckIntoStatistics(string byCodeDir, string statisticsDir, 
            bool exportTxtToo)
        {
            byCodeDir.ProcessFiles(statisticsDir,
                exportTxtToo ? ExtractHelper.ExportModes.Both : ExtractHelper.ExportModes.Binary,
                Console.Out);
        }

        static void PrintWeightsToCsv(string statisticsDir, string csv)
        {
            var dc = DigioClassifyHelper.GetNewDigioClassifier(statisticsDir, 0.1, 0, 1, true, Console.Out);
            dc.PrintWeightsToCsv(csv);            
        }


        /// <summary>
        ///  Prepare current data for prediction analysis
        /// </summary>
        /// <param name="code">The code</param>
        /// <param name="srcDir">The source directory for the (mostly recent) by date data</param>
        /// <param name="dstPath">The dest path for the by code data</param>
        static void PreparePredict(string code, string srcDir, string dstPath)
        {
            var sfs = srcDir.GetStockFiles();
            var dses = sfs.GetDailyEntries(code);
            using (var sw = new StreamWriter(dstPath))
            {
                dses.OutputDailyStockEntries(sw);
            }
        }

        static void Predict(string code, string dcPath, string rawDir, TextWriter tw)
        {
            var sfs = rawDir.GetStockFiles();
            var required = StatisticPoint.FirstCentralDay + 1;
            var dses = sfs.GetDailyEntriesLast(code, required);
            var input = dses.ToList().SuckOnlyInput(StatisticPoint.FirstCentralDay);
            var fccm = new FixedConfinedCoreManager();
            var dc = new DigioClassifier();
            // TODO populate dc.CoreLists
            dc.Load(dcPath);
            fccm.Build(dc, code);
            var prediction = fccm.Predict(input);
            prediction.Export(tw);
        }

        static void Predict(string code, string dcPath, string historyPath, string inputPath, TextWriter tw)
        {
            var input = PredictHelper.GetInputAsStaticPoint(historyPath, inputPath);
            var fccm = new FixedConfinedCoreManager();
            var dc = new DigioClassifier();
            // TODO populate dc.CoreLists
            dc.Load(dcPath);
            fccm.Build(dc, code);
            var prediction = fccm.Predict(input);
            prediction.Export(tw);
        }

        private static void GenerateBatchFiles()
        {
            throw new NotImplementedException();
        }

        static void Main(string[] args)
        {
            try
            {
                var subcmd = args[0].ToLower();
                switch (subcmd)
                {
                    case "-genbatches":
                        GenerateBatchFiles();
                        break;
                    case "-reorganise":
                        // args[1]: by-date dir, args[2]: by-code dir
                        ReorganiseFiles(args[1], args[2]);
                        break;
                    case "-reorganise-inc":
                        // args[1]: by-date dir, args[2]: by-code dir
                        ReorganiseFiles(args[1], args[2], true);
                        break;
                    case "-suck-txt":
                        // args[1]: source time-series file directory, args[2]: target static point directory
                        SuckIntoStatistics(args[1], args[2], true);
                        break;
                    case "-suck":
                        SuckIntoStatistics(args[1], args[2], false);
                        break;
                    case "-weights":
                        PrintWeightsToCsv(args[1], args[2]);
                        break;
                    case "-prepare":
                        PreparePredict(args[1], args[2], args[3]);
                        break;
                    case "-predict":
                        if (args.Length == 4)
                        {
                            Predict(args[1], args[2], args[3], Console.Out);
                        }
                        else
                        {
                            // args.Length == 3
                            Predict(args[1], args[2], null, Console.Out);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} occurred: {1}", e.GetType().Name, e.Message);
            }
        }
    }
}
