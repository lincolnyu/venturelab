using System;

namespace VentureCommon.Helpers
{
    public static class StockRecordHelper
    {
        public static bool TryParseLine<TRecord>(string line, out string code, ref TRecord de) where TRecord : StockRecord, new()
        {
            var segs = line.Split(',');
            code = null;
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
            if (de == null)
            {
                de = new TRecord();
            }
            de.Open = open;
            de.Close = close;
            de.Low = low;
            de.High = high;
            de.Volume = vol;
            de.Date = date;
            return true;
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
    }
}
