using System.Collections.Generic;
using System.Linq;

namespace VentureCommon.Helpers
{
    public static class ExpertParser
    {
        public class Prediction
        {
            public int Days { get; set; }
            public double Y { get; set; }
            public double StdVar { get; set; }
        }

        public static IEnumerable<T> ParseSample<T>(string line) where T : Prediction, new()
        {
            var segs = line.Split(';');
            foreach (var seg in segs)
            {
                var pair = seg.Split(':');
                if (pair.Length != 2) yield break;
                var expectDays = pair[0].Trim();
                var expectRange = pair[1].Trim();
                if (string.IsNullOrWhiteSpace(expectDays) || string.IsNullOrWhiteSpace(expectRange)) yield break;
                int days;
                expectDays = expectDays.TrimStart('+');
                if (!int.TryParse(expectDays, out days)) yield break;
                var i = expectRange.IndexOf("+/-");
                if (i < 0) yield break;
                var expectY = expectRange.Substring(0, i);
                var expectVar = expectRange.Substring(i + 3);
                if (string.IsNullOrWhiteSpace(expectY) || string.IsNullOrWhiteSpace(expectVar)) yield break;
                if (expectVar[expectVar.Length - 1] == '%') expectVar = expectVar.Substring(0, expectVar.Length - 1);
                double y, v;
                if (!double.TryParse(expectY, out y)) yield break;
                if (!double.TryParse(expectVar, out v)) yield break;
                yield return new T
                {
                    Days = days,
                    Y = y / 100,
                    StdVar = v / 100
                };
            }
        }

        public static bool ParseAsHeader(this string line, out int rank, out string code)
        {
            rank = 0;
            code = null;
            var i = line.IndexOf(':');
            if (i <= 0) return false;
            var expectRank = line.Substring(0, i);
            if (!int.TryParse(expectRank, out rank)) return false;
            var expectCode = line.Substring(i + 1).Trim();
            if (!IsStockCode(expectCode)) return false;
            code = expectCode;
            return true;
        }

        // TODO review this
        private static bool IsStockCode(string expectCode) => expectCode.Length == 3 && expectCode.All(char.IsLetterOrDigit);
    }
}
