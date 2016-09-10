using System;

namespace VentureVisualization.Samples
{
    public class GapSample : IDatedSample
    {
        public DateTime Date { get; set; }

        public double Step { get; set; } = 1;
    }
}
