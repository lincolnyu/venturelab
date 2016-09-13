using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VentureVisualization.Samples;
using VentureCommon.Helpers;

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
                if (line.ParseAsHeader(out rank, out c))
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

        private static IEnumerable<PredictionSample> ParseSample(string line)
        {
            var prevDays = 0;
            var samples = ExpertParser.ParseSample<PredictionSample>(line);
            foreach (var sample in samples)
            {
                sample.Step = sample.Days - prevDays;
                prevDays = sample.Days;
                yield return sample;
            }
        }
    }
}
