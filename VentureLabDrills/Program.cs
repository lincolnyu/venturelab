using QLogger.ConsoleHelpers;
using System.Linq;
using VentureLab.Asx;
using System;
using static QLogger.AppHelpers.AppInfo;
using static VentureLab.Helpers.AsxFileHelper;
using static VentureLab.Asx.StockManager;
using System.IO;
using VentureLab.Helpers;
using VentureLab.QbClustering;
using VentureLab.Prediction;
using VentureLabDrills.Output;
using VentureLab.QbGaussianMethod.Cores;
using VentureLab.QbGaussianMethod.Helpers;
using static VentureLab.QbGaussianMethod.Helpers.PredictionCommon;

namespace VentureLabDrills
{
    class Program
    {
        private static readonly string[] _periodNames = new[] { "+1", "+2", "+5", "+10", "+20", "+65" };

        private static MyLogger Logger;

        static void Main(string[] args)
        {
            if (args.Contains("--help"))
            {
                PrintHelp();
                return;
            }
            InitLogWithDisplayLevel(args);
            StockManager stockManager;
            IPointManager pointManager;
            Logger.WriteLine(MyLogger.Levels.Verbose, "Warming up...");
            if (!WarmUp(args, out stockManager, out pointManager))
            {
                Logger.WriteLine(MyLogger.Levels.Error, "The program failed to warm up.");
                return;
            }
            Logger.WriteLine(MyLogger.Levels.Verbose, "Warmed up.");
            var dateStr = args.GetSwitchValue("--date");

            var expertLenStr = args.GetSwitchValue("--expert");
            if (expertLenStr != null)
            {
                int expLen;
                int.TryParse(expertLenStr, out expLen);
                RunExpert(stockManager, pointManager, dateStr, expLen);
                return;
            }

            var code = args.GetSwitchValue("--predict");
            if (code != null && dateStr != null)
            {
                Predict(stockManager, pointManager, code, dateStr);
            }
            else
            {
                Logger.WriteLine(MyLogger.Levels.Info, "Prediction session started");
                PredictionSession(stockManager, pointManager);
            }
        }

        private static void RunExpert(StockManager stockManager, IPointManager pointManager, string dateStr, int expLen)
        {
            var list = Expert.Run(stockManager, pointManager, GaussianPredictionHelper.Predict, GetGiicb(dateStr));
            if (expLen > 0)
            {
                list = list.GetRange(0, expLen);
            }
            for (var i = 0; i < list.Count; i++)
            {
                var item = list[i];
                Console.WriteLine($"{i + 1}: {item.Item.Stock.Code}");
                DisplayPrediction(item.Y, item.YY);
                Console.WriteLine();
            }
        }

        private static void InitLogWithDisplayLevel(string[] args)
        {
            MyLogger.Levels displayLevel = MyLogger.Levels.Warning;
            if (args.Contains("--displaylevel=verbose"))
            {
                displayLevel = MyLogger.Levels.Verbose;
            }
            else if (args.Contains("--displaylevel=error"))
            {
                displayLevel = MyLogger.Levels.Error;
            }
            Logger = new MyLogger(displayLevel);
        }

        private static void PredictionSession(StockManager stockManager, IPointManager pointManager)
        {
            while (true)
            {
                Console.Write("Options (P - Predict; E - Expert; Q - Quit): ");
                var k = Console.ReadKey(false);
                Console.WriteLine();
                var kc = char.ToUpper(k.KeyChar);
                if (kc == 'N')
                {
                    break;
                }
                switch (kc)
                {
                    case 'P':
                        {
                            Console.Write("Code: ");
                            var code = Console.ReadLine();
                            Console.Write("Date (YYYYMMDD): ");
                            var dateStr = Console.ReadLine();
                            Predict(stockManager, pointManager, code, dateStr);
                            break;
                        }
                    case 'E':
                        {
                            Console.Write("Top count: ");
                            var expertLenStr = Console.ReadLine();
                            int expLen;
                            int.TryParse(expertLenStr, out expLen);
                            Console.Write("Date (YYYYMMDD): ");
                            var dateStr = Console.ReadLine();
                            RunExpert(stockManager, pointManager, dateStr, expLen);
                            break;
                        }
                }
            }
        }

        private static void Predict(StockManager stockManager, IPointManager pointManager, string code, string dateStr)
        {
            var cb = GetGiicb(dateStr);
            Predict(stockManager, pointManager, code, cb);
        }

        private static GetItemIndexCallback GetGiicb(string dateStr)
        {
            DateTime date;
            GetItemIndexCallback cb;
            if (dateStr != null && TryParseCompactDateString(dateStr, out date))
            {
                cb = GetDateCallbackClosure(date);
            }
            else
            {
                cb = GetLastOne;
            }
            return cb;
        }

        private static void Predict(StockManager stockManager, IPointManager pointManager, string code, GetItemIndexCallback giicb)
        {
            StockItem item;
            if (!stockManager.Items.TryGetValue(code, out item))
            {
                Console.WriteLine("Specified stock not found");
                return;
            }
#if SUPPRESS_SCORING
            var points = item.Points.Cast<GaussianRegulatedCore>().ToList();
            foreach (var p in points) p.Weight = 1;
#else
            var points = stockManager.PreparePrediction(item).Cast<GaussianRegulatedCore>().ToList();
#endif
            GaussianRegulatedCore.SetCoreParameters(points);
            var firstPoint = points.FirstOrDefault();
            DisplayParameters(firstPoint);
            DisplayWeights(item);
            if (firstPoint == null)
            {
                Console.WriteLine("Specified stock has no statistical points");
            }
            var outputLen = firstPoint.OutputLength;
            var y = new double[outputLen];
            var yy = new double[outputLen];

            var err = false;
            var predres = GaussianPredictionHelper.Predict(stockManager, pointManager, item,
                  x => {
                      var index = giicb(x);
                      if (index < 0)
                      {
                          err = true;
                          Console.WriteLine("Couldn't retrieve stock entry");
                      }
                      var day0 = item.Stock.Data[index];
                      Logger.WriteLine(MyLogger.Levels.Info, $"Predicting {day0.Date}");
                      return index;
                  }, y, yy);
            if (err) return;

            if (predres)
            {
                Logger.WriteLine(MyLogger.Levels.Info, "Prediction = {");
                DisplayPrediction(y, yy, 2);
                Logger.WriteLine(MyLogger.Levels.Info, "}");
            }
            else
            {
                Logger.WriteLine(MyLogger.Levels.Info, "Not enough points to be statistically significant");
            }
        }

        private static void DisplayPrediction(double[] y, double[] yy, int verticalIndent = -1)
        {
            for (var i = 0; i < y.Length && i < yy.Length; i++)
            {
                var yi = y[i];
                var yyi = yy[i];
                var stdyi = StatisticsHelper.GetStandardVariance(yi, yyi);
                var yiperc = (Math.Pow(2, yi) - 1) * 100;
                var pmperc = (Math.Pow(2, stdyi) - 1) * 100;
                if (verticalIndent >= 0)
                {
                    var indent = new string(' ', verticalIndent);
                    Logger.WriteLine(MyLogger.Levels.Info, $"{indent}{_periodNames[i]}: {yiperc:0.00}+/-{pmperc:0.00}%");
                }
                else
                {
                    Logger.Write(MyLogger.Levels.Info, $"{_periodNames[i]}: {yiperc:0.00}+/-{pmperc:0.00}%; ");
                }
            }
        }

        private static void DisplayWeights(StockItem item, int count = 5)
        {
            Logger.WriteLine(MyLogger.Levels.Verbose, "Weights = {");
            foreach (var kvp in item.Weights.Where(x => x.Value > 0).OrderByDescending(x=>x.Value).Take(count))
            {
                Logger.WriteLine(MyLogger.Levels.Verbose, $"  {kvp.Key.Stock.Code}: {kvp.Value:0.00}");
            }
            Logger.WriteLine(MyLogger.Levels.Verbose, "}");
        }

        private static void DisplayParameters(GaussianRegulatedCore firstPoint)
        {
            Logger.Write(MyLogger.Levels.Verbose, "L = { ");
            foreach (var l in firstPoint.L)
            {
                Logger.Write(MyLogger.Levels.Verbose, $"{l:0.0000} ");
            }
            Logger.WriteLine(MyLogger.Levels.Verbose, "}");
            Logger.Write(MyLogger.Levels.Verbose, "K = { ");
            foreach (var k in firstPoint.K)
            {
                Logger.Write(MyLogger.Levels.Verbose, $"{k:0.0000} ");
            }
            Logger.WriteLine(MyLogger.Levels.Verbose, "}");
        }
        

        private static int GetLastOne(StockItem item) => item.Stock.Data.Count - 1;

        private static GetItemIndexCallback GetDateCallbackClosure(DateTime date)
        {
            GetItemIndexCallback cb = item =>
            {
                var index = item.Stock.GetInsertIndex(date);
                if (index >= item.Stock.Data.Count) index = item.Stock.Data.Count - 1;
                return index;
            };
            return cb;
        }

        private static bool WarmUp(string[] args, out StockManager stockManager, out IPointManager pointManager)
        {
            try
            {
                stockManager = LoadStocks(args);
                SetUpStockManager(args, stockManager, out pointManager);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong:");
                Console.WriteLine($"{e.GetType()}:");
                Console.WriteLine($"{e.Message}");
                Console.WriteLine("Check you command and input or see the following for detailed usage.");
                PrintHelp();
                stockManager = null;
                pointManager = null;
                return false;
            }
        }
        
        private static StockManager LoadStocks(string[] args)
        {
            var inputFolder = args.GetSwitchValue("-i");
            var dir = new DirectoryInfo(inputFolder);
            var fromStr = args.GetSwitchValue("--from");
            var toStr = args.GetSwitchValue("--to");
            var stockManager = new StockManager();
            if (fromStr != null || toStr != null)
            {
                DateTime dateFrom, dateTo;
                if (!TryParseCompactDateString(fromStr, out dateFrom))
                {
                    dateFrom = DateTime.MinValue;
                }
                if (!TryParseCompactDateString(toStr, out dateTo))
                {
                    dateTo = DateTime.MaxValue;
                }
                stockManager.Load(dir, dateFrom, dateTo);
            }
            else
            {
                stockManager.Load(dir);
            }
            return stockManager;
        }

        private static void SetUpStockManager(string[] args, StockManager stockManager, out IPointManager pointManager)
        {
            var adapterStr = args.GetSwitchValue("--adapter");
            StrainAdapter adapter;
            var isAbs = adapterStr == "abs";
            if (isAbs)
            {
                adapter = AbsStrainAdapter.Instance;
            }
            else
            {
                adapter = SqrStrainAdapter.Instance;
            }
            var inputThrStr = args.GetSwitchValue("--inputthr");
            var outputThrStr = args.GetSwitchValue("--outputthr");
            var outputPenaltyStr = args.GetSwitchValue("--outputpenalty");
            double defaultInputThr = 0.1 * (isAbs ? SampleAccessor.InputCount : Math.Sqrt(SampleAccessor.InputCount));
            double defaultOutputThr = 0.1 * (isAbs ? SampleAccessor.OutputCount : Math.Sqrt(SampleAccessor.OutputCount));
            double inputThr, outputThr;
            if (!double.TryParse(inputThrStr, out inputThr))
            {
                inputThr = defaultInputThr;
            }
            if (!double.TryParse(outputThrStr, out outputThr))
            {
                outputThr = defaultOutputThr;
            }
            double outputPenalty;
            if (!double.TryParse(outputPenaltyStr, out outputPenalty))
            {
                outputPenalty = SimpleScorer.DefaultOutputPenalty;
            }

            var scorer = new SimpleScorer(inputThr, 1.0/outputThr, outputPenalty);
            pointManager = new GaussianStockPoint.Manager {
                M = 100,
                N = 1
            };
            var parallel = args.Contains("-p");
            if (parallel)
            {
                stockManager.ReloadStrainsParallel(pointManager);
            }
            else
            {
                stockManager.ReloadStrains(pointManager);
            }

#if !SUPPRESS_SCORING

            ScoreTable st;
            string loadTable;
            if ((loadTable = args.GetSwitchValue("--loadScoreTable")) != null)
            {
                Logger.Write(MyLogger.Levels.Info, $"Loading score table from file {loadTable}...");
                st = new ScoreTable();
                using (var sr = new StreamReader(loadTable))
                {
                    st.Load(stockManager, sr);
                }
                Logger.WriteLine(MyLogger.Levels.Info, "done.");
            }
            else
            {
                Logger.LocateInplaceWrite();
                if (parallel)
                {
                    st = stockManager.GetScoreTableParallel(adapter, scorer, ReportGetScoresProgress);
                }
                else
                {
                    st = stockManager.GetScoreTable(adapter, scorer, ReportGetScoresProgress);
                }
                Logger.InplaceWriteLine(MyLogger.Levels.Info);
                string saveTable;
                if ((saveTable = args.GetSwitchValue("--saveScoreTable")) != null)
                {
                    Logger.Write(MyLogger.Levels.Info, $"Saving score table to file {saveTable}...");
                    using (var sw = new StreamWriter(saveTable))
                    {
                        st.Save(sw);
                    }
                    Logger.WriteLine(MyLogger.Levels.Info, "done.");
                }
            }
            stockManager.SetScoreTableToItems(st);
#endif
        }

        private static void ReportGetScoresProgress(int done, int total)
        {
            Logger.InplaceWrite(MyLogger.Levels.Info, $"{done}/{total} scores done.");
        }

        private static void PrintHelp()
        {
            var appname = GetAppExecutableName();
            var appver = GetAppVersion();
            Console.WriteLine($"VentureLab (Ver {appver})");
            Console.WriteLine("Usage: ");
            Console.WriteLine($"  {appname} -i <input folder> [--from <inclusive starting date>] [--to <exclusive ending date>]");
            Console.WriteLine("     [--displaylevel={error|warning|verbose}] ");
            Console.WriteLine($"  {appname} --help");
        }
    }
}
