using System.Collections.Generic;
using VentureVisualization.Samples;

namespace VentureVisualization.SequencePlotting
{
    using DrawPredictionDelegate = SequencePlotter.DrawShapeDelegate<PredictionPlotter.PredictionShape>;

    public class PredictionPlotter : SequencePlotter
    {
        public class PredictionShape : BaseShape
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double YUpper { get; set; }
            public double YLower { get; set; }
            public double PrevX { get; set; }
            public double PrevY { get; set; }
        }

        public PredictionPlotter(CandleChartPlotter candleChartPlotter) : base(true)
        {
            CandleChartPlotter = candleChartPlotter;
        }

        public YMarginManager YMarginManager => CandleChartPlotter.YMarginManager;
        public double YMax => YMarginManager.YMax;
        public double YMin => YMarginManager.YMin;

        public CandleChartPlotter CandleChartPlotter { get; }
        public double ChartWidth => CandleChartPlotter.ChartWidth;
        public double ChartHeight => CandleChartPlotter.ChartHeight;
        public override YModes YMode => CandleChartPlotter.YMode;
        public double Length => Sequencer.Length;

        public event DrawPredictionDelegate DrawPrediction;

        public override void PreDraw(IEnumerable<ISample> samples, double startSlot = 0)
        {
            double? lastClose = null;
            PlotLoop(samples, startSlot, (s, slot) =>
            {
                var p = s as PredictionSample;
                if (p != null)
                {
                    if (lastClose == null) return false;
                    var yupper = lastClose.Value * (1 + p.Y + p.StdVar);
                    var ylower = lastClose.Value * (1 + p.Y - p.StdVar);
                    YMarginManager.UpdateMax(yupper);
                    YMarginManager.UpdateMin(ylower);
                    return true;
                }
                var r = s as RecordSample;
                if (r != null)
                {
                    lastClose = r.Close;
                }
                return true;
            });
        }

        public override void Draw(IEnumerable<ISample> samples, double startSlot = 0)
        {
            var yd = YMax - YMin;
            var xrate = ChartWidth / Length;
            var yrate = ChartHeight / yd;
            double? lastClose = null;
            double prevSlot = 0;
            double prevY = 0;
            PlotLoop(samples, startSlot, (s, slot) =>
            {
                var p = s as PredictionSample;
                if (p != null)
                {
                    // not preceded by end of records, not to draw predict
                    if (lastClose == null)
                    {
                        return false;
                    }
                    var x = xrate * slot;
                    var yval = lastClose * (1 + p.Y);
                    var yupperval = lastClose * (1 + p.Y + p.StdVar);
                    var ylowerval = lastClose * (1 + p.Y - p.StdVar);

                    var y = (yval - YMin) * yrate;
                    var yupper = (yupperval - YMin) * yrate;
                    var ylower = (ylowerval - YMin) * yrate;

                    if (YMode == YModes.TopToBottom)
                    {
                        yupper = ChartHeight - yupper;
                        ylower = ChartHeight - ylower;
                    }

                    var shape = new PredictionShape
                    {
                        X = x,
                        Y = y.Value,
                        YUpper = yupper.Value,
                        YLower = ylower.Value,
                        PrevX = prevSlot * xrate,
                        PrevY = prevY
                    };
                    DrawPrediction(shape);

                    prevY = y.Value;
                    prevSlot = slot;
                    return true;
                }
                var r = s as RecordSample;
                if (r != null)
                {
                    prevSlot = slot;
                    lastClose = prevY = r.Close;
                }
                return true;
            });
        }
    }
}
