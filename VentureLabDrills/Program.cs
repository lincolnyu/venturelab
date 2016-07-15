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

namespace VentureLabDrills
{
    class Program
    {
        private delegate int GetItemIndexCallback(StockItem item);

        static void Main(string[] args)
        {
            if (args.Contains("-h"))
            {
                PrintHelp();
                return;
            }
            StockManager stockManager;
            IPointManager pointManager;
            if (!WarmUp(args, out stockManager, out pointManager))
            {
                return;
            }
            var code = args.GetSwitchValue("--predict");
            var dateStr = args.GetSwitchValue("--predictdate");
            if (code != null)
            {
                Predict(stockManager, pointManager, code, dateStr);
            }
            else
            {
                PredictionSession(stockManager, pointManager);
            }
        }

        private static void PredictionSession(StockManager stockManager, IPointManager pointManager)
        {
            while (true)
            {
                Console.Write("Code: ");
                var code = Console.ReadLine();
                Console.Write("Date (YYYYMMDD): ");
                var dateStr = Console.ReadLine();
                Predict(stockManager, pointManager, code, dateStr);
                Console.Write("Continue? (Y/n)");
                var k = Console.ReadKey(false);
                if (char.ToUpper(k.KeyChar) == 'N')
                {
                    break;
                }
            }
        }

        private static void Predict(StockManager stockManager, IPointManager pointManager, string code, string dateStr)
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
            Predict(stockManager, pointManager, code, cb);
        }

        private static void Predict(StockManager stockManager, IPointManager pointManager, string code, GetItemIndexCallback giicb)
        {
            StockItem item;
            if (!stockManager.Items.TryGetValue(code, out item))
            {
                Console.WriteLine("Specified stock not found");
                return;
            }
            var points = stockManager.PreparePrediction(item);
            var firstPoint = points.FirstOrDefault();
            if (firstPoint == null)
            {
                Console.WriteLine("Specified stock has no statistical points");
            }
            var outputLen = firstPoint.OutputLength;
            var y = new double[outputLen];
            var yy = new double[outputLen];
            var index = giicb(item);
            if (index < 0)
            {
                Console.WriteLine("Couldn't retrieve stock entry");
                return;
            }
            var input = item.SampleInput(pointManager, index);
            pointManager.GetExpectedY(y, input.StrainPoint.Input, points);
            pointManager.GetExpectedY(yy, input.StrainPoint.Input, points);
            Console.Write("(E[Y], VAR[Y]) = ( ");
            for (var i = 0; i < y.Length && i < yy.Length; i++)
            {
                var yi = y[i];
                var yyi = yy[i];
                var varyi = yyi - yi * yi;
                Console.Write($"({yi}, {varyi}) ");
            }
            Console.WriteLine(")");
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
            if (adapterStr == "abs")
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
            const double defaultInputThr = 0.1;
            const double defaultOutputThr = 0.1;
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
            var scorer = new SimpleScorer(inputThr, outputThr, outputPenalty);
            pointManager = new GaussianStockPoint.Manager();
            var parallel = args.Contains("-p");
            if (parallel)
            {
                stockManager.ReloadStrainsParallel(pointManager);
                stockManager.UpdateScoreTableParallel(adapter, scorer);
            }
            else
            {
                stockManager.ReloadStrains(pointManager);
                stockManager.UpdateScoreTable(adapter, scorer);
            }
        }

        private static void PrintHelp()
        {
            var appname = GetAppExecutableName();
            var appver = GetAppVersion();
            Console.WriteLine($"VentureLab (Ver {appver})");
            Console.WriteLine("Usage: ");
            Console.WriteLine($"  {appname} -i <input folder> [--from <inclusive starting date>] [--to <exclusive ending date>]");
            Console.WriteLine($"  {appname} --help");
        }
    }
}
