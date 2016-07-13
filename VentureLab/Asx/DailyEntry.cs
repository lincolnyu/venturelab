using System;

namespace VentureLab.Asx
{
    public class DailyEntry : IComparable<DailyEntry>
    {
        public DateTime Date { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }

        public int CompareTo(DailyEntry other)
        {
            return Date.CompareTo(other.Date);
        }
    }
}
