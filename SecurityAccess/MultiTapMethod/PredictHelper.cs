using SecurityAccess.Asx;
using System;
using System.Linq;

namespace SecurityAccess.MultiTapMethod
{
    public static class PredictHelper
    {
        #region Methods

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

            var yielded = buffer.EnumerateDailyStockEntryLoopBuffer(0, required, inputEntryCount);
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
