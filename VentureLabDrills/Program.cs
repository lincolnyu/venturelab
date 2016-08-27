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
using static VentureLab.QbGaussianMethod.Helpers.PredictionCommon;
using QLogger.Logging;
using VentureLab;
using VentureLab.QbGaussianMethod.Helpers;

namespace VentureLabDrills
{
    class Program
    {
        private class PointManagerFactory : IPointManagerFactory
        {
            public PointManagerFactory(double m = DefaultM, double n = DefaultN)
            {
                M = m;
                N = n;
                ReusableManager = (StockPoint.Manager)Create();
            }

            public double M { get; }

            public double N { get; }

            public StockPoint.Manager ReusableManager { get; }

            #region IPointManagerFactory members

            public IPointManager Create()
            {
                var pointManager = new StockPoint.Manager();
                pointManager.PrepareCoreCreation(M, N);
                return pointManager;
            }

            #endregion

            /// <summary>
            ///  Reset reusable manager as core creator for new sequential prediction
            /// </summary>
            /// <returns></returns>
            public StockPoint.Manager ResetReusableManager()
            {
                ReusableManager.PrepareCoreCreation(M, N);
                return ReusableManager;
            }
        }

        private const double DefaultM = 100;
        private const double DefaultN = 1;

        private static SimpleTimeEstimator _simpleTimeEstimator = new SimpleTimeEstimator();

        private static PointManagerFactory _pointManagerFactory;

        private static readonly string[] _periodNames = new[] { "+1", "+2", "+5", "+10", "+20", "+65" };

        private static MyLogger Logger;

        public const int MaxDaysAheadAllowed = 5;
        public const int MaxDaysBehindAllowed = 5;

        static void Main(string[] args)
        {
            _pointManagerFactory = new PointManagerFactory();
            if (args.Contains("--help"))
            {
                PrintHelp();
                return;
            }

            var st = args.GetSwitchValue("--fix");
            if (st != null)
            {
                if (File.Exists(st))
                {
                    FixStoreTableFile(st);
                }
                else
                {
                    Console.WriteLine("Cannot find the file to fix.");
                }
                return;
            }

            InitLogWithDisplayLevel(args);
            SetupLogOutput(args);

            StockManager stockManager;
            Logger.WriteLine(MyLogger.Levels.Verbose, "Warming up...");
            if (!WarmUp(args, out stockManager))
            {
                Logger.WriteLine(MyLogger.Levels.Error, "The program failed to warm up.");
                return;
            }
            Logger.WriteLine(MyLogger.Levels.Verbose, "Warmed up.");

            if (args.Contains("--warmupOnly"))
            {
                return;
            }

            var dateStr = args.GetSwitchValue("--date");
            var expertLenStr = args.GetSwitchValue("--expert");
            if (expertLenStr != null)
            {
                var pnum = args.GetMaxDegreeOfParallelism();
                int expLen;
                int.TryParse(expertLenStr, out expLen);
                RunExpert(stockManager, dateStr, expLen, pnum);
                return;
            }

            var code = args.GetSwitchValue("--predict");
            if (code != null && dateStr != null)
            {
                Predict(stockManager, _pointManagerFactory.ReusableManager, code, dateStr);
            }
            else
            {
                Logger.WriteLine(MyLogger.Levels.Info, "Prediction session started");
                PredictionSession(stockManager, _pointManagerFactory.ReusableManager);
            }
        }

        private static void FixStoreTableFile(string stsrc)
        {
            var stfixed = stsrc + ".fix";
            using (var sr = new StreamReader(stsrc))
            {
                using (var sw = new StreamWriter(stfixed))
                {
                    ScoreTableFixer.Fix(sr, sw);
                }
            }
        }

        private static void RunExpert(StockManager stockManager, string dateStr, int expLen, int parallel)
        {
            Logger.LocateInplaceWrite();
            _simpleTimeEstimator.Start();
            var list = parallel > 1?
                Expert.RunParallel(stockManager, _pointManagerFactory, GaussianOneOffPredictor.Instance.Predict, GetGiicb(dateStr), MaxDaysAheadAllowed, ReportExpertProgress, parallel): 
                Expert.Run(stockManager, _pointManagerFactory.ReusableManager, GaussianOneOffPredictor.Instance.Predict, GetGiicb(dateStr), MaxDaysAheadAllowed, ReportExpertProgress);
            Logger.WriteLine(MyLogger.Levels.Info);

            if (expLen > 0)
            {
                list = list.GetRange(0, expLen);
            }
            for (var i = 0; i < list.Count; i++)
            {
                var item = list[i];
                Logger.WriteLine(MyLogger.Levels.Info, $"{i + 1}: {item.Item.Stock.Code}");
                DisplayPrediction(item.Y, item.YY);
                Logger.WriteLine(MyLogger.Levels.Info);
            }
        }

        private static void InitLogWithDisplayLevel(string[] args)
        {
            MyLogger.Levels displayLevel = MyLogger.Levels.Warning;
            if (args.Contains("--displayLevel=verbose"))
            {
                displayLevel = MyLogger.Levels.Verbose;
            }
            else if (args.Contains("--displayLevel=error"))
            {
                displayLevel = MyLogger.Levels.Error;
            }
            Logger = new MyLogger(displayLevel);
        }

        private static void SetupLogOutput(string[] args)
        {
            MyLogger.Levels logLevel = MyLogger.Levels.Verbose;
            if (args.Contains("--logLevel=warning"))
            {
                logLevel = MyLogger.Levels.Warning;
            }
            else if (args.Contains("--logLevel=error"))
            {
                logLevel = MyLogger.Levels.Error;
            }
            var outfile = args.GetSwitchValue("--log");
            if (outfile != null)
            {
                var fw = new FileWriter(Logger, new StreamWriter(outfile))
                {
                    IsEnabled = true,
                    FlushEveryWrite = true,
                    ThresholdLevel = logLevel
                };
                Logger.Writers.Add(fw);
            }
        }

        private static void PredictionSession(StockManager stockManager, StockPoint.Manager pointManager)
        {
            while (true)
            {
                Console.Write("Options (P - Predict; E - Expert; Q - Quit): ");
                var k = Console.ReadKey(false);
                Console.WriteLine();
                var kc = char.ToUpper(k.KeyChar);
                if (kc == 'Q')
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
                            Console.Write("Degree of Parallelism: ");
                            var pstr = Console.ReadLine();
                            int pnum = 1;
                            int.TryParse(pstr, out pnum);
                            RunExpert(stockManager, dateStr, expLen, pnum);
                            break;
                        }
                }
            }
        }

        private static void Predict(StockManager stockManager, StockPoint.Manager pointManager, string code, string dateStr)
        {
            var cb = GetGiicb(dateStr);
            code = code.ToUpper();
            Predict(stockManager, code, cb);
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

        private static void Predict(StockManager stockManager, string code, GetItemIndexCallback giicb)
        {
            StockItem item;
            if (!stockManager.Items.TryGetValue(code, out item))
            {
                Logger.WriteLine(MyLogger.Levels.Error, "Specified stock not found");
                return;
            }

            var pointManager = _pointManagerFactory.ResetReusableManager();

#if SUPPRESS_SCORING
            var cores = pointManager.CreateCores(item.Points).Cast< GaussianRegulatedCore>().ToList();
#else
            var cores = stockManager.PreparePrediction(item, pointManager).Cast<GaussianRegulatedCore>().ToList();
#endif
            var firstPoint = cores.FirstOrDefault();
            if (firstPoint == null)
            {
                Logger.WriteLine(MyLogger.Levels.Error, "Specified stock has no statistical points.");
                return;
            }

            var outputLen = firstPoint.Point.OutputLength;
            var result = new PredictionResult(outputLen);
            GaussianOneOffPredictor.Instance.Predict(stockManager, pointManager, item,
                  x => {
                      var index = giicb(x);
                      if (index >= 0)
                      {
                          var day0 = item.Stock.Data[index];
                          Logger.WriteLine(MyLogger.Levels.Info, $"Predicting {day0.Date}");
                      }
                      return index;
                  }, result);
            if (result.ErrorMessage == null)
            {
                DisplayParameters(firstPoint);
                DisplayWeights(item);
                Logger.WriteLine(MyLogger.Levels.Info, $"Totally {cores.Count} statistical points generated for {code}.");

                Logger.WriteLine(MyLogger.Levels.Info, "Prediction = {");
                DisplayPrediction(result.Y, result.YY, 2);
                Logger.WriteLine(MyLogger.Levels.Info, "}");
            }
            else
            {
                Logger.WriteLine(MyLogger.Levels.Error, result.ErrorMessage);
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

        private static void DisplayParameters(GaussianRegulatedCore core)
        {
            Logger.Write(MyLogger.Levels.Verbose, "L = { ");
            foreach (var l in core.Variables.L)
            {
                Logger.Write(MyLogger.Levels.Verbose, $"{l:0.0000} ");
            }
            Logger.WriteLine(MyLogger.Levels.Verbose, "}");
            Logger.Write(MyLogger.Levels.Verbose, "K = { ");
            foreach (var k in core.Variables.K)
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
                if (index >= 0 && index < item.Stock.Data.Count)
                {
                    var s = item.Stock.Data[index];
                    if (s.Date < date.Subtract(TimeSpan.FromDays(MaxDaysAheadAllowed))
                        || s.Date > date.Subtract(TimeSpan.FromDays(MaxDaysBehindAllowed)))
                    {
                        return -1;
                    }
                }
                return index;
            };
            return cb;
        }

        private static bool WarmUp(string[] args, out StockManager stockManager)
        {
            try
            {
                stockManager = LoadStocks(args);
                SetUpStockManager(args, stockManager);
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

        private static void SetUpStockManager(string[] args, StockManager stockManager)
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
          
            double outputPenalty;
            if (!double.TryParse(outputPenaltyStr, out outputPenalty))
            {
                outputPenalty = SimpleScorer.DefaultOutputPenalty;
            }

            var pointManager = _pointManagerFactory.ReusableManager;

            var parallel = args.GetMaxDegreeOfParallelism();
            if (parallel > 1)
            {
                stockManager.ReloadStrainsParallel(pointManager, parallel);
            }
            else
            {
                stockManager.ReloadStrains(pointManager);
            }

#if SUPPRESS_SCORING
            stockManager.SetupDefaultWeights();
#else
            string loadTable;
            if ((loadTable = args.GetSwitchValue("--loadScoreTable")) != null)
            {
                Logger.Write(MyLogger.Levels.Info, $"Loading score table from file {loadTable}...");
                using (var sr = new StreamReader(loadTable))
                {
                    stockManager.LoadScoresDirect(sr);
                }
                Logger.WriteLine(MyLogger.Levels.Info, "done.");
            }
            else
            {
                Logger.LocateInplaceWrite();
                _simpleTimeEstimator.Start();

                double inputThr, outputThr;
                if (!double.TryParse(inputThrStr, out inputThr))
                {
                    inputThr = defaultInputThr;
                }
                if (!double.TryParse(outputThrStr, out outputThr))
                {
                    outputThr = defaultOutputThr;
                }
                var scorer = new SimpleScorer(inputThr, 1.0 / outputThr, outputPenalty);
                var inc = args.GetSwitchValueAsInt("--scoreSampleInc", 1, 1);
                ScoreTable st;
                if (parallel > 1)
                {
                    st = stockManager.GetScoreTableParallel(adapter, scorer, inc, ReportGetScoresProgress, parallel);
                }
                else
                {
                    st = stockManager.GetScoreTable(adapter, scorer, inc, ReportGetScoresProgress);
                }
                Logger.WriteLine(MyLogger.Levels.Info);
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
                stockManager.SetScoreTableToItems(st);
            }
            stockManager.SuppressCrossScores();
#endif
        }

        private static void ReportGetScoresProgress(int done, int total)
        {
            ReportProgress(done, total, "scores");
            Logger.InplaceWrite(MyLogger.Levels.Info, $"{done}/{total} scores done.");
        }

        private static bool ReportExpertProgress(Expert.Result result, int done, int total)
        {
            ReportProgress(done, total, "stocks");
            return true;
        }

        private static bool ReportProgress(int done, int total, string itemName)
        {
            var percentage = (double)done / total;
            _simpleTimeEstimator.Report(percentage);
            var elapsed = _simpleTimeEstimator.TotalElapsed;
            var remain = _simpleTimeEstimator.Estimate;
            var elapsedstr = elapsed.ToString(@"d\.hh\:mm\:ss");
            var remainstr = remain?.ToString(@"d\.hh\:mm")?? "unestimatable time";
            Logger.InplaceWrite(MyLogger.Levels.Info, $"{done}/{total} {itemName} done, {elapsedstr} elapsed {remainstr} remaining.");
            return true;
        }

        private static void PrintHelp()
        {
            var appname = GetAppExecutableName();
            var appver = GetAppVersion();
            Console.WriteLine();
            Console.WriteLine($"VentureLab (Ver {appver})");
            Console.WriteLine("(c) 2016 quanbenSoft. All rights reserved.");
            Console.WriteLine();
            Console.WriteLine("Usage: ");
            Console.WriteLine();
            Console.WriteLine($"  {appname} -i <input folder> [--from <inclusive starting date>] [--to <exclusive ending date>]");
            Console.WriteLine("       [--{saveScoreTable|loadScoreTable} <score table file path>]");
            Console.WriteLine("       [--scoreSampleInc <step of sampling iteration for score table, default 1>] ");
            Console.WriteLine("       [--displayLevel={error|warning|verbose}]");
            Console.WriteLine("       [-p [<degree of maximum parallelism, default being infinity>]]");
            Console.WriteLine("       [--{predict <code name>|expert}]");
            Console.WriteLine("       [--logLevel={error|warning|verbose}]");
            Console.WriteLine("       [--log <log file path]");
            Console.WriteLine();
            Console.WriteLine("    Perform the stock price prediction functionality");
            Console.WriteLine("    * '--predict' to predict a specified stock");
            Console.WriteLine("    * '--expert' for expert mode that automatically recommends stocks");
            Console.WriteLine("    * When neither is used, enter the interactive prediction session loop ");
            Console.WriteLine();
            Console.WriteLine($"  {appname} --fix <score table file path>");
            Console.WriteLine();
            Console.WriteLine("    Attempt to fix the score file (such that it is compliant to the current version).");
            Console.WriteLine("    Fixed path will be suffixed by '.fix'.");
            Console.WriteLine();
            Console.WriteLine($"  {appname} --help");
            Console.WriteLine();
            Console.WriteLine("    Display this help ");
            Console.WriteLine();
        }
    }
}
