using System;
using System.Collections.Generic;

namespace VentureLab.Asx
{
    public class Stock
    {
        public Stock(string code)
        {
            Code = code;
        }

        public string Code { get; }
        public string Description { get; set; }
        public List<DailyEntry> Data { get; } = new List<DailyEntry>();

        public bool Add(DailyEntry de)
        {
            if (Data.Count == 0 || Data[Data.Count - 1].CompareTo(de) < 0)
            {
                Data.Add(de);
            }
            else
            {
                var index = Data.BinarySearch(de);
                if (index >= 0) return false;
                index = -index - 1;
                Data.Insert(index, de);
            }
            return true;
        }

        public int GetIndex(DateTime date)
        {
            var dummy = new DailyEntry { Date = date.Date };
            var index = Data.BinarySearch(dummy);
            return index;
        }

        public int GetInsertIndex(DateTime date)
        {
            var index = GetIndex(date);
            return index >= 0 ? index : -index - 1;
        }
    }
}
