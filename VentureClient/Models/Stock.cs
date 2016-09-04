using System.Collections.Generic;
using System.Threading.Tasks;
using VentureCommon;
using static VentureCommon.Helpers.StockRecordHelper;

namespace VentureClient.Models
{
    public class Stock : FileBased
    {
        public delegate void StockUpdatedEventHandler();

        public string Code { get; private set; }

        public List<StockRecord> Data { get; } = new List<StockRecord>();

        public event StockUpdatedEventHandler StockUpdated;

        public async Task LoadFromFile()
        {
            await PickFile();
            var lines = ParseFile();
            LoadFromLines(lines);
        }

        private void LoadFromLines(IEnumerable<string> lines)
        {
            Data.Clear();
            foreach (var line in lines)
            {
                StockRecord entry = null;
                string code;
                if (TryParseLine(line, out code, ref entry))
                {
                    Code = code;
                    Data.Add(entry);
                }
            }
            StockUpdated?.Invoke();
        }
    }
}
