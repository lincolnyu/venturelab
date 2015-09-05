using System;
using System.Collections.Generic;

namespace SecurityAccess.Asx
{
    public static class InterpolationHelper
    {
        #region Nested types

        public interface IInterpolatable : IOutputFile
        {
            #region Properties

            int MissingDays { get; set; }

            DailyStockEntry LastAvailable { get; set; }

            #endregion
        }

        #endregion

        #region Enumerations

        public enum InterpolationModes
        {
            None,
            Linear,
            Extend,
        }

        #endregion

        #region Methods

        public static void Interpolate(InterpolationModes mode,
            IList<DateTime> dates, IInterpolatable outFile, DailyStockEntry dsd)
        {
            switch (mode)
            {
                case InterpolationModes.Linear:
                    {
                        for (var i = 0; i < outFile.MissingDays; i++)
                        {
                            var r2 = (i + 1) / (outFile.MissingDays + 1);
                            var r1 = 1 - r2;
                            var close = outFile.LastAvailable.Close * r1 + dsd.Open * r2;
                            var open = close;
                            var low = close;
                            var high = close;
                            outFile.WriteDailyStockEntry(dsd.Code, dates[dates.Count - outFile.MissingDays + i - 1],
                                open, high, low, close, 0.0);
                        }
                        break;
                    }
                case InterpolationModes.Extend:
                    {
                        for (var i = 0; i < outFile.MissingDays; i++)
                        {
                            var close = outFile.LastAvailable.Close;
                            var open = close;
                            var low = close;
                            var high = close;
                            outFile.WriteDailyStockEntry(dsd.Code, dates[dates.Count - outFile.MissingDays + i - 1],
                                open, high, low, close, 0.0);
                        }
                        break;
                    }
            }
        }

        #endregion
    }
}
