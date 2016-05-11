using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SecurityAccess.Asx
{
    /// <summary>
    ///  helper class to retrieve ASX history data provided by
    ///  http://www.asxhistoricaldata.com/
    /// </summary>
    public static class HistoryHelper
    {
        #region Methods

        public static string GetFileName(this string dir, DateTime date)
        {
            var fn = date.DateToString();
            return Path.Combine(dir, fn);
        }

        public static bool IsAsxHistoryFile(this string fileName)
        {
            if (!Path.GetExtension(fileName).Equals(".txt", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            var fn = Path.GetFileNameWithoutExtension(fileName);
            if (fn.Length != 8)
            {
                return false;
            }
            foreach (var ch in fn)
            {
                if (!char.IsDigit(ch))
                {
                    return false;
                }
            }
            return true;
        }

        public static DateTime GetDateOfFile(this string fileName)
        {
            var fn = Path.GetFileNameWithoutExtension(fileName);
            return fn.StringToDate();
        }

        public static DateTime StringToDate(this string str)
        {
            var syear = str.Substring(0, 4);
            var smonth = str.Substring(4, 2);
            var sday = str.Substring(6, 2);
            var year = int.Parse(syear);
            var month = int.Parse(smonth);
            var day = int.Parse(sday);
            var dt = new DateTime(year, month, day);
            return dt;
        }

        public static string DateToString(this DateTime date)
        {
            return $"{date.Year:0000}{date.Month:00}{date.Day:00}";
        }

        public static string GetOutputFileName(this string dir, string code)
        {
            var fn = $"{code}.txt";
            return Path.Combine(dir, fn);
        }

        /// <summary>
        ///  Reads entries from stock sequence file and put them in the cyclic buffer
        /// </summary>
        /// <param name="fn">The file to read from</param>
        /// <param name="buffer">The buffer</param>
        /// <param name="bufferStart">The start of the buffer</param>
        /// <param name="bufferEnd">The end of the buffer</param>
        /// <param name="i">current point the data is written to</param>
        /// <returns>number of days read</returns>
        public static int ReadDailyStockEntriesToBuffer(this string fn, 
            DailyStockEntry[] buffer, int bufferStart, int bufferEnd, ref int i)
        {
            var entries = fn.ReadDailyStockEntries();
            var c = 0;
            foreach (var entry in entries)
            {
                if (i >= bufferEnd)
                {
                    i = bufferStart;
                }
                buffer[i] = entry;
                i++;
                c++;
            }
            return c;
        }

        public static IEnumerable<DailyStockEntry> EnumerateDailyStockEntryLoopBuffer(
            this IList<DailyStockEntry> buffer, int bufferStart, int bufferEnd, int start)
        {
            for (var i = start; i < bufferEnd; i++)
            {
                yield return buffer[i];
            }
            for (var i = bufferStart; i < start; i++)
            {
                yield return buffer[i];
            }
        }

        /// <summary>
        ///  Reads dse sequence from the specified file (normally a by date raw file but
        ///  it doesn't have to, as far as it uses the standard format)
        /// </summary>
        /// <param name="fn">The file</param>
        /// <returns>The ordered list of entries read from the file</returns>
        public static IEnumerable<DailyStockEntry> ReadDailyStockEntries(this string fn)
        {
            using (var sr = new StreamReader(fn))
            {
                return sr.ReadDailyStockEntries();
            }
        }

        public static IEnumerable<DailyStockEntry> ReadDailyStockEntries(this StreamReader sr)
        {
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                var data = line.ReadDailyStockEntry();
                if (data != null)
                {
                    yield return data;
                }
            }
        } 

        public static DailyStockEntry ReadDailyStockEntry(this string line)
        {
            var columns = line.Split(',');
            if (columns.Length != 7)
            {
                return null;
            }
            var dse = new DailyStockEntry
            {
                Code = columns[0],
                Date = columns[1].StringToDate(),
                Open = double.Parse(columns[2]),
                High = double.Parse(columns[3]),
                Low = double.Parse(columns[4]),
                Close = double.Parse(columns[5]),
                Volume = double.Parse(columns[6])
            };
            return dse;
        }

        public static void WriteDailyStockEntry(this StreamWriter output, DailyStockEntry dse)
        {
            output.WriteDailyStockEntry(dse.Code, dse.Date, dse.Open, dse.High, dse.Low,
                dse.Close, dse.Volume);
        }

        public static void WriteDailyStockEntry(this StreamWriter output, string code, 
            DateTime date, double open, double high, double low, double close, double volume)
        {
            var dateStr = date.DateToString();
            var line = $"{code},{dateStr},{open},{high},{low},{close},{volume}";
            output.WriteLine(line);
        }

        public static async Task WriteDailyStockEntryAsync(this StreamWriter output, DailyStockEntry dse)
        {
            await output.WriteDailyStockEntryAsync(dse.Code, dse.Date, dse.Open, dse.High, dse.Low,
                dse.Close, dse.Volume);
        }

        public static async Task WriteDailyStockEntryAsync(this StreamWriter output, string code,
           DateTime date, double open, double high, double low, double close, double volume)
        {
            var dateStr = date.DateToString();
            var line = $"{code},{dateStr},{open},{high},{low},{close},{volume}";
            await output.WriteLineAsync(line);
        }

        public static DateTime GetNextWeekDay(this DateTime day)
        {
            DateTime nextDay;
            if (day.DayOfWeek == DayOfWeek.Friday)
            {
                nextDay = day.AddDays(3);
            }
            else if (day.DayOfWeek == DayOfWeek.Saturday)
            {
                nextDay = day.AddDays(2);
            }
            else
            {
                nextDay = day.AddDays(1);
            }
            return nextDay;
        }

        public static DateTime GetPrevWeekDay(this DateTime day)
        {
            DateTime prevDay;
            if (day.DayOfWeek == DayOfWeek.Monday)
            {
                prevDay = day.AddDays(-3);
            }
            else if (day.DayOfWeek == DayOfWeek.Sunday)
            {
                prevDay = day.AddDays(-2);
            }
            else
            {
                prevDay = day.AddDays(-1);
            }
            return prevDay;
        }

        #endregion
    }
}
