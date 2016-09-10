using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VentureVisualization.Samples;

namespace VentureClient.Models
{
    public class Expert : FileBased
    {
        public delegate void ExpertUpdatedEventHandler();

        public string Code { get; private set; }

        public Dictionary<string, List<PredictionSample>> Data { get; } = new Dictionary<string, List<PredictionSample>>();

        public event ExpertUpdatedEventHandler ExpertUpdated;

        public async Task LoadFromFile()
        {
            var filePicked = await PickFile();
            if (filePicked)
            {
                var lines = ParseFile();
                LoadFromLines(lines);
            }
        }

        private void LoadFromLines(IEnumerable<string> lines)
        {
            Data.Clear();
            int rank;
            string code = null;
            foreach (var line in lines)
            {
                string c;
                if (ParseAsHeader(line, out rank, out c))
                {
                    code = c;
                    continue;
                }
                else if (code != null)
                {
                    var samples = ParseSample(line).ToList();
                    if (samples.Count > 1)
                    {
                        Data[code] = samples;
                    }
                    code = null;
                }
            }
            ExpertUpdated?.Invoke();
        }

        private static bool ParseAsHeader(string line, out int rank, out string code)
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

        private static IEnumerable<PredictionSample> ParseSample(string line)
        {
            var segs = line.Split(';');
            var prevDays = 0;
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
                yield return new PredictionSample
                {
                    Step = days - prevDays,
                    Y = y/100,
                    StdVar = v/100
                };
                prevDays = days;
            }
        }

        // TODO review this
        private static bool IsStockCode(string expectCode) =>  expectCode.Length == 3 && expectCode.All(char.IsLetterOrDigit);

    }
}
