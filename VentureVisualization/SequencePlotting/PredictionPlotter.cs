using System;
using System.Collections.Generic;

namespace VentureVisualization.SequencePlotting
{
    public class PredictionPlotter : SequencePlotter
    {
        public class PredictionPoint
        {
            public TimeSpan Time { get; set; }
            public double Y { get; set; }
            public double StdVar { get; set; }
        }

        public PredictionPlotter(CandleChartPlotter candleChartPlotter) : base(true)
        {
            CandleChartPlotter = candleChartPlotter;
        }

        public CandleChartPlotter CandleChartPlotter { get; }
        public double ChartWidth => CandleChartPlotter.ChartWidth;
        public double ChartHeight => CandleChartPlotter.ChartHeight;
        public YModes YMode => CandleChartPlotter.YMode;
        public StockSequencer Sequencer => CandleChartPlotter.Sequencer;
        public double Length => Sequencer.Length;

        public override void Draw(IEnumerable<ISample> samples, double startSlot = 0)
        {
            /*
            var yd = YMax - YMin;
            var xrate = ChartWidth / Length;
            var yrate = ChartHeight / yd;
            double lastClose = 0;
            PlotLoop(samples, startSlot, (s, slot) =>
            {
                var p = s as PredictionSample;
                if (p != null)
                {
                    var x = xrate * slot;

                    var y = yrate * (p.Y - YMin);
                    var yupper = ChartHeight * (p)
                    return;
                }
                var r = s as RecordSample;
                if (r != null)
                {
                    lastClose = r.Close;
                }
            }
            */
        }



#if false

        /// <summary>
        ///  Draw the prediction
        /// </summary>
        /// <param name="startSlot">The slot of the last known record</param>
        /// <param name="points">The prediction points</param>
        public void Draw(IEnumerable<PredictionPoint> points)
        {
            //var startSlot = 
            if (!PreDraw(startSlot, points)) return;
            var lastClose = Sequencer.Data[Sequencer.Data.Count - 1].Close;
            var slot = startSlot;
            var yd = YMax - YMin;
            var xrate = ChartWidth / Length;
            var yrate = ChartHeight / yd;
            foreach (var p in points)
            {
                slot += p.Time.TotalDays;
                if (slot >= Length) break;
                var x = xrate * slot;
                var y = yrate * (p.Y - YMin);
                var yupper = ChartHeight * (p)
            }

        }


        /// <summary>
        ///  Prepares for draw and returns if can draw
        /// </summary>
        /// <returns></returns>
        private bool PreDraw()
        {
            foreach (var p in points)
        }
#endif
    }
}
