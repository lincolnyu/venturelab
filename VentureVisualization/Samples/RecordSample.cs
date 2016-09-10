using System;
using VentureCommon;

namespace VentureVisualization.Samples
{
    public class RecordSample : StockRecord, IDatedSample
    {
        public double Offset => 0;

        public double Step { get; set; } = 1;
    }
}
