using System;
using System.Collections.Generic;
using System.IO;

namespace SecurityAccess.Asx
{
    /// <summary>
    ///  helper class to retrieve ASX history data provided by
    ///  http://www.asxhistoricaldata.com/
    /// </summary>
    public static class HistoryHelper
    {
        #region Methods

        public static string GetFileName(string dir, DateTime date)
        {
            var fn = string.Format("{0}{1:00}{2:00}.txt", date.Year, date.Month, date.Day);
            return Path.Combine(dir, fn);
        }
        
        public static IEnumerable<DailyStockdata> Load(this string fn)
        {
            DailyStockdata data = new DailyStockdata();
            using (var sr = new StreamReader(fn))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (line.Load(data))
                    {
                        yield return data;
                        data = new DailyStockdata();
                    }
                }
            }
        }

        public static bool Load(this string line, DailyStockdata data)
        {
            var columns = line.Split(',');
            if (columns.Length != 7)
            {
                return false;
            }
            data.Code = columns[0];
            data.Open = double.Parse(columns[2]);
            data.High = double.Parse(columns[3]);
            data.Low = double.Parse(columns[4]);
            data.Close = double.Parse(columns[5]);
            data.Volume = double.Parse(columns[6]);
            return true;
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
