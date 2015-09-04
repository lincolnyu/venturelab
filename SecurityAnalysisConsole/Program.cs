using System;
using SecurityAccess;
using SecurityAccess.Asx;
using SecurityAccess.MultiTapMethod;
using GaussianCore.Generic;
using System.IO;

namespace SecurityAnalysisConsole
{
    class Program
    {
        static void ReorganiseFiles(string srcDir, string dstDir, bool append=false)
        {
            var fr = new FileReorganiser(srcDir, dstDir, Console.Out, append);
            fr.Reorganise();
        }

        static void SuckIntoStatistic(string srcDir, string dstDir)
        {
            ExtractHelper.ProcessFiles(srcDir, dstDir, Console.Out);
        }

        static FixedConfinedBuilder BuildFixedConfined(string selectionFile, string srcDir, string savePath)
        {
            var coreManager = new FixedConfinedCoreManager();
            var builder = new FixedConfinedBuilder(coreManager);
            builder.LoadCodeSelection(selectionFile);
            builder.Build(srcDir);
            builder.Save(savePath);
            return builder;
        }

        static void Predict(string dataPath, string inputPath, string historyPath, TextWriter tw)
        {
            var input = PredictHelper.GetInputAsStaticPoint(historyPath, inputPath);
            var coreManager = new FixedConfinedCoreManager();
            var builder = new FixedConfinedBuilder(coreManager);
            builder.Load(dataPath);
            var prediction = coreManager.Predict(input);
            prediction.Export(tw);
        }

        static void Main(string[] args)
        {
            try
            {
                if (args[0].Equals("-reorganise", StringComparison.OrdinalIgnoreCase))
                {
                    // args[1]: src, args[2]: dst
                    ReorganiseFiles(args[1], args[2]);
                }
                else if (args[0].Equals("-reorganise-inc", StringComparison.OrdinalIgnoreCase))
                {
                    // args[1]: src, args[2]: dst
                    ReorganiseFiles(args[1], args[2], true);
                }
                else if (args[0].Equals("-suck", StringComparison.OrdinalIgnoreCase))
                {
                    SuckIntoStatistic(args[1], args[2]);
                }
                else if (args[0].Equals("-build-fixedconfined", StringComparison.OrdinalIgnoreCase))
                {
                    var builder = BuildFixedConfined(args[1], args[2], args[3]);
                    if (args.Length > 4)
                    {
                        builder.ExportToText(args[4]);
                    }
                }
                else if (args[0].Equals("-predict", StringComparison.OrdinalIgnoreCase))
                {
                    Predict(args[1], args[2], args[3], Console.Out);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} occurred: {1}", e.GetType().Name, e.Message);
            }
        }
    }
}
