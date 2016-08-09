using System;
using System.IO;
using System.Linq;
using VentureLab.Asx;

namespace VentureLab.Helpers
{
    public static class AsxFileHelper
    {
        /// <summary>
        ///  Loads stock data from files that contain stocks of the day
        /// </summary>
        /// <param name="stockManager">The stock manager to load the stocks to</param>
        /// <param name="dir">The directory where the files are kept</param>
        public static void Load(this StockManager stocks, DirectoryInfo dir)
        {
            var files = dir.GetFiles("*.txt").OrderBy(x => x.Name);
            foreach (var file in files)
            {
                DateTime dt;
                if (!IsAsxDateFile(file.Name, out dt)) continue;
                LoadFile(stocks, file);
            }
        }

        /// <summary>
        ///  Loads stock data from files that contain stocks of the day in the specified date range
        /// </summary>
        /// <param name="stockManager">The stock manager to load the stocks to</param>
        /// <param name="dir">The directory where the files are kept</param>
        /// <param name="from">Starting date inclusive</param>
        /// <param name="to">Ending date exclusive</param>
        public static void Load(this StockManager stockManager, DirectoryInfo dir, DateTime from, DateTime to)
        {
            var files = dir.GetFiles("*.txt").OrderBy(x => x.Name);
            foreach (var file in files)
            {
                DateTime dt;
                if (!IsAsxDateFile(file.Name, out dt)) continue;
                if (dt < from) continue;
                if (dt >= to) break;
                LoadFile(stockManager, file);
            }
        }

        public static void LoadFile(this StockManager stocks, FileInfo file)
        {
            using (var fs = file.OpenRead())
            using (var sr = new StreamReader(fs))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (line == null) break;
                    string code;
                    DailyEntry de;
                    if (!TryParseLine(line, out code, out de)) continue;
                    stocks.Add(code, de);
                }
            }
        }

        public static bool TryParseCompactDateString(string dateStr, out DateTime date)
        {
            date = default(DateTime);
            if (dateStr == null || dateStr.Length != 8) return false;
            var yrstr = dateStr.Substring(0, 4);
            var mthstr = dateStr.Substring(4, 2);
            var daystr = dateStr.Substring(6, 2);
            int yr, mth, day;
            if (!int.TryParse(yrstr, out yr)) return false;
            if (!int.TryParse(mthstr, out mth)) return false;
            if (!int.TryParse(daystr, out day)) return false;
            date = new DateTime(yr, mth, day);
            return true;
        }

        public static bool IsAsxDateFile(string fileName, out DateTime date)
        {
            var fnwoext = Path.GetFileNameWithoutExtension(fileName);
            return TryParseCompactDateString(fnwoext, out date);
        }

        private static bool TryParseLine(string line, out string code, out DailyEntry de)
        {
            var segs = line.Split(',');
            code = null;
            de = null;
            if (segs.Length != 7) return false;
            code = segs[0];
            double open, close, low, high, vol;
            DateTime date;
            if (!TryParseCompactDateString(segs[1], out date)) return false;
            if (!double.TryParse(segs[2], out open)) return false;
            if (!double.TryParse(segs[3], out high)) return false;
            if (!double.TryParse(segs[4], out low)) return false;
            if (!double.TryParse(segs[5], out close)) return false;
            if (!double.TryParse(segs[6], out vol)) return false;
            de = new DailyEntry
            {
                Open = open,
                Close = close,
                Low = low,
                High = high,
                Volume = vol,
                Date = date
            };
            return true;
        }

        private static bool TryParseCompactDateString(string v, out object dt)
        {
            throw new NotImplementedException();
        }
    }
}
