using VentureCommon;

namespace VentureVisualization.Samples
{
    public class RecordSample : StockRecord, IDatedSample
    {
        public double Step { get; set; } = 1;
    }
}
