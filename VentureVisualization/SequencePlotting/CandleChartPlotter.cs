using System;
using System.Collections.Generic;

namespace VentureVisualization.SequencePlotting
{
    using Samples;
    using DrawCandleDelegate = SequencePlotter.DrawShapeDelegate<CandleChartPlotter.CandleShape>;

    public sealed class CandleChartPlotter : SamplePlotter
    {
        #region Nested types

        public enum CandleTypes
        {
            Yin,
            Yang
        }

        public class CandleShape : BaseShape
        {
            public CandleTypes CandleType { get; set; }
            public double X { get; set; }
            public double YMin { get; set; }
            public double YMax { get; set; }
            public double XRectMin { get; set; }
            public double XRectMax { get; set; }
            public double YRectMin { get; set; }
            public double YRectMax { get; set; }
        }

        #endregion

        #region Default values

        public const double DefaultYinWidthRatio = 0.8;
        public const double DefaultYangWidthRatio = 0.8;

        #endregion

        /// <summary>
        ///  Creates a candel chart
        /// </summary>
        /// <param name="data">The data reference held by this drawer</param>
        /// <param name="chartWidth">The width of the chart</param>
        /// <param name="chartHeight">The height of the chart</param>
        public CandleChartPlotter(YMarginManager yMarginManager) : base(true)
        {
            YMarginManager = yMarginManager;
        }
      
        #region Chart config

        public double ChartWidth { get; set; }
        public double ChartHeight { get; set; }

        /// <summary>
        ///  Width of yang bar vs width of its allocated horizontal slot
        /// </summary>
        public double YangWidthRatio { get; set; } = DefaultYangWidthRatio;

        /// <summary>
        ///  Width of yin bar vs width of its allocated horizontal slot
        /// </summary>
        public double YinWidthRatio { get; set; } = DefaultYinWidthRatio;

        #endregion

        public YMarginManager YMarginManager { get; }

        #region Drawing delegate

        public event DrawCandleDelegate DrawCandle;

        #endregion

        public override void PreDraw(IEnumerable<VentureVisualization.ISample> samples, double startSlot = 0)
        {
            PlotLoop(samples, startSlot, (s, slot) =>
            {
                var r = s as RecordSample;
                if (r == null) return true;
                YMarginManager.UpdateMax(r.High);
                YMarginManager.UpdateMin(r.Low);
                return true;
            });
        }

        public override void Draw(IEnumerable<VentureVisualization.ISample> samples, double startSlot = 0)
        {
            var ymin = YMarginManager.YMin;
            var ymax = YMarginManager.YMax;

            FireDrawBegin();

            var yd = ymax - ymin;
            PlotLoop(samples, startSlot, (s, slot) =>
            {
                var r = s as RecordSample;
                if (r == null) return true;
                var low = ChartHeight * r.Low;
                
                var yopen = (r.Open - ymin) * ChartHeight / yd;
                var yclose = (r.Close - ymin) * ChartHeight / yd;
                var candleMax = (r.High - ymin) * ChartHeight / yd;
                var candleMin = (r.Low - ymin) * ChartHeight / yd;
                var ct = yopen < yclose ? CandleTypes.Yang : CandleTypes.Yin;

                var x1 = ChartWidth * slot / Sequencer.Length;
                var x2 = ChartWidth * (slot + 1) / Sequencer.Length;
                var x = (x1 + x2) / 2;
                var widthRatio = ct == CandleTypes.Yin ? YinWidthRatio : YangWidthRatio;
                x1 = x - (x - x1) * widthRatio;
                x2 = x + (x2 - x) * widthRatio;

                var rectymin = Math.Min(yopen, yclose);
                var rectymax = Math.Max(yopen, yclose);
                if (YMode == YModes.TopToBottom)
                {
                    var temp = ChartHeight - rectymax;
                    rectymax = ChartHeight - rectymin;
                    rectymin = temp;
                    temp = ChartHeight - candleMax;
                    candleMax = ChartHeight - candleMin;
                    candleMin = temp;
                }
                var irectymin = (int)Math.Round(rectymin);
                var irectymax = (int)Math.Round(rectymax);
                var cs = new CandleShape
                {
                    Record = r,
                    CandleType = ct,
                    X = x,
                    XRectMin = x1,
                    XRectMax = x2,
                    YRectMin = rectymin,
                    YRectMax = rectymax,
                    YMin = candleMin,
                    YMax = candleMax
                };
                DrawCandle(cs);
                return true;
            });

            FireDrawEnd();
        }
    }
}
