using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static SecurityAccess.Asx.InterpolationHelper;

namespace SecurityAccess.Asx
{
    public static class SingleReorganiseHelper
    {
        #region Nested types

        public class OutputFile : IInterpolatable
        {
            #region 

            public DailyStockEntry LastAvailable
            {
                get; set;
            }

            public int MissingDays
            {
                get; set;
            }

            public StreamWriter Writer { get; set; }

            #endregion

            #region Methods

            public void WriteDailyStockEntry(DailyStockEntry dse)
            {
                Writer.WriteDailyStockEntry(dse);
            }

            public void WriteDailyStockEntry(string code, DateTime date, double open, double high, double low, double close, double volume)
            {
                Writer.WriteDailyStockEntry(code, date, open, high, low, close, volume);
            }

            #endregion
        }

        #endregion

        #region Methods

        /// <summary>
        ///  returns a list of raw data file (files named by date containing all stock data on that day)
        /// </summary>
        /// <param name="srcDir">The folder that contains such files</param>
        /// <returns>The list of all such files in ascending order by name (therefore date)</returns>
        public static IEnumerable<FileInfo> GetStockFiles(this string srcDir)
        {
            var dir = new DirectoryInfo(srcDir);
            var files = dir.GetFiles();
            return files.Where(x => x.Name.IsAsxHistoryFile()).OrderBy(x => x.Name);
        }

        public static IEnumerable<DateTime> GetDates(this IEnumerable<FileInfo> srcFiles)
        {
            return srcFiles.Select(x => x.Name.GetDateOfFile());
        }

        public static IEnumerable<DailyStockEntry> GetDailyEntries(this IEnumerable<FileInfo> srcFiles, string code)
        {
            return srcFiles.SelectMany(file => file.FullName.ReadDailyStockEntries().Where(x=>x.Code.Equals(code, StringComparison.OrdinalIgnoreCase)));
        }

        public static IEnumerable<DailyStockEntry> GetDailyEntriesLast(this IEnumerable<FileInfo> srcFiles, string code,
            int lastCount)
        {
            var rev = srcFiles.Reverse();
            var res = rev.GetDailyEntries(code).Reverse();
            return res.TakeWhile(e => --lastCount > 0);
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="dses">DSEs of a specified stock ordered by date</param>
        /// <param name="sw"></param>
        /// <param name="dates"></param>
        /// <param name="mode"></param>
        public static void OutputDailyStockEntries(this IEnumerable<DailyStockEntry> dses,
            StreamWriter sw, IList<DateTime> dates = null, InterpolationModes mode =
            InterpolationModes.None)
        {
            // only used for interpolation
            var outFile = new OutputFile { Writer = sw };
            var i = 0;
            foreach (var dse in dses)
            {
                if (mode != InterpolationModes.None)
                {
                    var currDate = dse.Date;
                    var missingDays = 0;
                    for (; dates[i] < currDate; i++)
                    {
                        missingDays++;
                    }
                    outFile.MissingDays = missingDays;
                    Interpolate(mode, dates, outFile, dse);
                    outFile.LastAvailable = dse;
                    i++;
                }

                outFile.WriteDailyStockEntry(dse);
            }
            if (mode != InterpolationModes.None && i < dates.Count)
            {
                outFile.MissingDays = dates.Count - i;
                Interpolate(mode, dates, outFile, null);
            }
        }

        #endregion
    }
}
