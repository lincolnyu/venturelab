using System;
using System.IO;
using System.Linq;
using VentureCommon;
using VentureLab.Asx;
using static VentureCommon.Helpers.StockRecordHelper;

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
                    StockRecord de = null;
                    if (!TryParseLine(line, out code, ref de)) continue;
                    stocks.Add(code, de);
                }
            }
        }

        public static bool IsAsxDateFile(string fileName, out DateTime date)
        {
            var fnwoext = Path.GetFileNameWithoutExtension(fileName);
            return TryParseCompactDateString(fnwoext, out date);
        }

        public static string GetCodeOfLine(string line)
        {
            var segs = line.Split(',');
            if (segs.Length < 1) return null;
            return segs[0];
        }
    }
}
