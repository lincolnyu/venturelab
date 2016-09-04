using System;

namespace VentureCommon
{
    public class StockRecord : IComparable<StockRecord>
    {
        public DateTime Date { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }

        public int CompareTo(StockRecord other)
        {
            return Date.CompareTo(other.Date);
        }
    }
}
