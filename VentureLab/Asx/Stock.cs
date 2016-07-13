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
    }
}
