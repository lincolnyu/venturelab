using SecurityAccess.Asx;
using System;
using System.IO;
using System.Linq;
using SecurityAccess.Helpers;

namespace SecurityAccess.MultiTapMethod
{
    public static class PredictHelper
    {
        #region Methods

        public static StatisticPoint GetInputAt(string code, DateTime firstDay, 
            string byDateDir, string byCodeDir)
        {
            var len = StatisticPoint.FirstCentralDay + 1;
            // gets from code file first if it can as it only requires to read one file
            // it's assumed the by code file most likely contains date no later than the earlist of the history files
            var codeFilePath = Path.Combine(byCodeDir, code + ".txt");
            var fromCode = codeFilePath.ReadDailyStockEntries().Where(x => x.Date >= firstDay)
                .Take(len);
            var fcList = fromCode.ToList();
            if (fcList.Count >= len)
            {
                // we've got all we need
                return fcList.GetRange(0, len).SuckOnlyInput(0);
            }
            string dayStr;
            if (fcList.Count > 0)
            {
                var first = fcList.First().Date;
                if (first > firstDay)
                {
                    // see if by date has possibly any earlier data
                    var firstDayStr = firstDay.DateToString();
                    var readtoDayStr = first.DateToString();
                    var dir1 = new DirectoryInfo(byDateDir);
                    var front = dir1.GetStockEntriesFromDate(code, firstDayStr)
                        .TakeWhile(x => x.Date.CompareTo(readtoDayStr) < 0);
                    fcList = front.Concat(fcList).ToList();
                    if (fcList.Count >= len)
                    {
                        return fcList.GetRange(0, len).SuckOnlyInput(0);
                    }
                }
                var last = fcList.Last().Date.AddDays(1);
                dayStr = last.DateToString();
            }
            else
            {
                dayStr = firstDay.DateToString();
            }
            var shortfall = len - fcList.Count;
            // gets the rest from the by date file
            var dir = new DirectoryInfo(byDateDir);
            var append = dir.GetStockEntriesFromDate(code, dayStr).Take(shortfall);
            fcList = fcList.Concat(append).ToList();
            return fcList.Count < len ? null : fcList.SuckOnlyInput(0);
        }

        /// <summary>
        ///  Returns the point that represents the current loaded from files
        /// </summary>
        /// <param name="historyPath">The path to the history</param>
        /// <param name="inputPath">The input path that extends the history if any</param>
        /// <returns>The current point based on which predict value is to be calculated</returns>
        public static StatisticPoint GetInputAsStaticPoint(string historyPath, string inputPath = null)
        {
            var required = StatisticPoint.FirstCentralDay + 1;
            var buffer = new DailyStockEntry[required];
            var i = 0;
            var inputEntryCount = 0;
            if (!string.IsNullOrWhiteSpace(inputPath))
            {
                inputEntryCount = inputPath.ReadDailyStockEntriesToBuffer(buffer, 0, required, ref i);
                if (inputEntryCount >= required)
                {
                    return buffer.SuckOne(i);
                }
            }

            // inputEntryCount == i
            var requiredHistory = required - inputEntryCount;
            var historyEntryCount = historyPath.ReadDailyStockEntriesToBuffer(buffer, inputEntryCount, 
                inputEntryCount + requiredHistory, ref i);

            if (historyEntryCount < requiredHistory)
            {
                return null;// not enough data
            }

            var yielded = buffer.EnumerateDailyStockEntryLoopBuffer(0, required, i);
            return yielded.ToList().SuckOnlyInput(StatisticPoint.FirstCentralDay);
        }

        public static Prediction Predict(this GaussianCore.ICoreManager coreManager,
            StatisticPoint spinput)
        {
            var input = new double[22];
            spinput.GenerateInput(input);

            var y0 = coreManager.GetExpectedY(input, 0);
            var y1 = coreManager.GetExpectedY(input, 1);
            var y2 = coreManager.GetExpectedY(input, 2);
            var y3 = coreManager.GetExpectedY(input, 3);
            var y4 = coreManager.GetExpectedY(input, 4);
            var y5 = coreManager.GetExpectedY(input, 5);

            var yy0 = coreManager.GetExpectedSquareY(input, 0);
            var yy1 = coreManager.GetExpectedSquareY(input, 1);
            var yy2 = coreManager.GetExpectedSquareY(input, 2);
            var yy3 = coreManager.GetExpectedSquareY(input, 3);
            var yy4 = coreManager.GetExpectedSquareY(input, 4);
            var yy5 = coreManager.GetExpectedSquareY(input, 5);

            var p = new Prediction
            {
                FP1 = y0,
                FP2 = y1,  // close
                FP5 = y2,  // avg of close and same as below
                FP10 = y3,
                FP20 = y4,
                FP65 = y5,

                // standard variance
                FP1Sv = Math.Sqrt(yy0 - y0 * y0),
                FP2Sv = Math.Sqrt(yy1 - y1 * y1),
                FP5Sv = Math.Sqrt(yy2 - y2 * y2),
                FP10Sv = Math.Sqrt(yy3 - y3 * y3),
                FP20Sv = Math.Sqrt(yy4 - y4 * y4),
                FP65Sv = Math.Sqrt(yy5 - y5 * y5),
            };

            return p;
        }

        #endregion
    }
}
