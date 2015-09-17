using System;
using SecurityAccess;
using SecurityAccess.Asx;
using SecurityAccess.MultiTapMethod;
using GaussianCore.Generic;
using System.IO;
using System.Collections.Generic;

namespace SecurityAnalysisConsole
{
    class Program
    {
        class ClassifierQuitter
        {
            public double SimilarThreshold { get; set; }

            public ClassifierQuitter(double similarThreshold)
            {
                SimilarThreshold = similarThreshold;
            }

            public bool Quit(SimilarityClassifier.DistanceEntry de)
            {
                var d = Math.Sqrt(de.SquareDistance);
                return d > SimilarThreshold;
            }
        }

        static void ReorganiseFiles(string byDateDir, string byCodeDir, bool append = false)
        {
            var fr = new FileReorganiser(byDateDir, byCodeDir, Console.Out, append);
            fr.Reorganise();
        }

        static void SuckIntoStatistics(string byCodeDir, string statisticsDir, 
            bool exportTxtToo)
        {
            ExtractHelper.ProcessFiles(byCodeDir, statisticsDir,
                exportTxtToo ? ExtractHelper.ExportModes.Both : ExtractHelper.ExportModes.Binary,
                Console.Out);
        }

        static void ClassifyNew(string statisticsDir, string similarsDir)
        {
            var classifier = new SimilarityClassifier(statisticsDir);
            var csets = classifier.GetCoreSets();
            var sdl = SimilarityClassifier.GetOrderedSquareDistances(csets);

            var cq = new ClassifierQuitter(0.2);
            classifier.Classify(csets, sdl, cq.Quit, similarsDir);
        }

        static void ClassifyInc(string statisticsDir, string similarsDir)
        {
            var classifier = new SimilarityClassifier(statisticsDir);
            classifier.ClassifyInc(similarsDir);
        }

        static FixedConfinedBuilder BuildFixedConfined(IList<string> codes, string srcDir, string savePath)
        {
            var coreManager = new FixedConfinedCoreManager();
            var builder = new FixedConfinedBuilder(coreManager);
            builder.BuildFromBinary(srcDir, codes);
            builder.Save(savePath);
            return builder;
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

        static void Predict(string dataPath, string historyPath, string inputPath, TextWriter tw)
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
                var subcmd = args[0].ToLower();
                switch (subcmd)
                {
                    case "-reorganise":
                        // args[1]: src, args[2]: dst
                        ReorganiseFiles(args[1], args[2], false);
                        break;
                    case "-reorganise-inc":
                        // args[1]: src, args[2]: dst
                        ReorganiseFiles(args[1], args[2], true);
                        break;
                    case "-suck-txt":
                        SuckIntoStatistics(args[1], args[2], true);
                        break;
                    case "-suck":
                        SuckIntoStatistics(args[1], args[2], false);
                        break;
                    case "-classify":
                        ClassifyNew(args[1], args[2]);
                        break;
                    case "-classify-inc":
                        ClassifyInc(args[1], args[2]);
                        break;



                    #region Naive prediction:...
                    case "-build-fc-sel":
                        {
                            var codes = FixedConfinedBuilder.LoadCodeSelection(args[1]);
                            var builder = BuildFixedConfined(codes, args[2], args[3]);
                            if (args.Length > 4)
                            {
                                builder.ExportToText(args[4]);
                            }
                            break;
                        }
                    case "-build-fc":
                        {
                            var builder = BuildFixedConfined(new[] { args[1] }, args[2], args[3]);
                            if (args.Length > 4)
                            {
                                builder.ExportToText(args[4]);
                            }
                            break;
                        }
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
                        #endregion
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} occurred: {1}", e.GetType().Name, e.Message);
            }
        }
    }
}
