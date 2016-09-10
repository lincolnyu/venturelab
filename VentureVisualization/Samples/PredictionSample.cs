using System;

namespace VentureVisualization.Samples
{
    public class PredictionSample : ITimeSpanSample
    {
        public double Step { get; set; }

        public TimeSpan TimeSpan { get; set; }

        public double Y { get; set; }
        public double StdVar { get; set; }
    }
}
