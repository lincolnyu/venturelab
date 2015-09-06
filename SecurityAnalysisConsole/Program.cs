using System;
using SecurityAccess;
using SecurityAccess.Asx;
using SecurityAccess.MultiTapMethod;
using GaussianCore.Generic;
using System.IO;
using System.Linq;

namespace SecurityAnalysisConsole
{
    class Program
    {
        static void ReorganiseFiles(string srcDir, string dstDir, bool append = false)
        {
            var fr = new FileReorganiser(srcDir, dstDir, Console.Out, append);
            fr.Reorganise();
        }

        static void SuckIntoStatistic(string srcDir, string dstDir, 
            bool exportTxtToo)
        {
            ExtractHelper.ProcessFiles(srcDir, dstDir,
                exportTxtToo ? ExtractHelper.ExportModes.Both : ExtractHelper.ExportModes.Binary,
                Console.Out);
        }

        static void BuildFixedConfinedForAll(string srcDir, string dstDir)
        {
          //  FixedConfinedBuilder.RebuildAll(srcDir, dstDir, true, Console.Out);
        }

        static FixedConfinedBuilder BuildFixedConfined(string selectionFile, string srcDir, string savePath)
        {
            var coreManager = new FixedConfinedCoreManager();
            var builder = new FixedConfinedBuilder(coreManager);
            var codes = FixedConfinedBuilder.LoadCodeSelection(selectionFile);
            builder.BuildFromBinary(srcDir, codes);
            builder.Save(savePath);
            return builder;
        }

        static void Predict(string dataPath, string historyPath, string inputPath, TextWriter tw)
        {
            var input = PredictHelper.GetInputAsStaticPoint(historyPath, inputPath);
            var coreManager = new FixedConfinedCoreManager();
            var builder = new FixedConfinedBuilder(coreManager);
            builder.Load(dataPath);
            var prediction = coreManager.Predict(input);
            prediction.Export(tw);
        }

        /// <summary>
        ///  Prepare current data for prediction analysis
        /// </summary>
        /// <param name="code">The code</param>
        /// <param name="srcPath">The source path for the by date data</param>
        /// <param name="dstPath">The dest path for the by code data</param>
        static void PreparePredict(string code, string srcPath, string dstPath)
        {
            var sfs = srcPath.GetStockFiles();
            var dses = sfs.GetDailyEntries(code);
            using (var sw = new StreamWriter(dstPath))
            {
                dses.OutputDailyStockEntries(sw);
            }
        }

        static void Main(string[] args)
        {
            try
            {
                var subcmd = args[0].ToLower();
                switch (subcmd)
                {
                    case "-reorganise":
                        // args[1]: src, args[2]: dst
                        ReorganiseFiles(args[1], args[2]);
                        break;
                    case "-reorganise-inc":
                        // args[1]: src, args[2]: dst
                        ReorganiseFiles(args[1], args[2], true);
                        break;
                    case "-suck-txt":
                        SuckIntoStatistic(args[1], args[2], true);
                        break;
                    case "-suck":
                        SuckIntoStatistic(args[1], args[2], false);
                        break;
                    case "-build-fc":
                        {
                            var builder = BuildFixedConfined(args[1], args[2], args[3]);
                            if (args.Length > 4)
                            {
                                builder.ExportToText(args[4]);
                            }
                            break;
                        }
                    case "-build-fc-all":
                        BuildFixedConfinedForAll(args[1], args[2]);
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
