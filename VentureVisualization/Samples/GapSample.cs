using System;

namespace VentureVisualization.Samples
{
    public class GapSample : IDatedSample
    {
        public DateTime Date { get; set; }

        public double Offset => 0;

        public double Step { get; set; } = 1;
    }
}
