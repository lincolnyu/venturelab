using System;

namespace SecurityAccess.Asx
{
    public class DailyStockEntry
    {
        #region Properties

        public string Code { get; set; }
        public DateTime Date { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }

        #endregion
    }
}
