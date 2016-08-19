using System.Collections.Generic;
using System.Linq;
using VentureLab.Asx;

namespace VentureLab.Helpers
{
    public static class StockManagerHelper
    {
        public static IEnumerable<string> GetAllCodes(this StockManager stockManager)
            => stockManager.Items.Select(x => x.Key);

        public static int GetOutputLength(this StockManager stockManager)
        {
            var point = stockManager.Items.Values.First(x => x.Points.Count > 0).Points.First();
            return point.OutputLength;
        }

        public static int GetInputLength(this StockManager stockManager)
        {
            var point = stockManager.Items.Values.First(x => x.Points.Count > 0).Points.First();
            return point.InputLength;
        }
    }
}
