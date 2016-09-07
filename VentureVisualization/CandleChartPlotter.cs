using System;
using System.Collections.Generic;
using VentureCommon;

namespace VentureVisualization
{
    using DrawCandleDelegate = StockPlotter.DrawShapeDelegate<CandleChartPlotter.CandleShape>;

    public sealed class CandleChartPlotter : StockPlotter
    {
        #region Nested types

        public enum CandleTypes
        {
            Yin,
            Yang
        }

        /// <summary>
        ///  Drawing modes
        /// </summary>
        /// <remarks>
        ///  Specified times:
        ///    
        ///  As-is:
        ///    When start time, count specified, end time is deduced
        ///    When end time, count specified, start time is deduced
        /// </remarks>
        public enum VerticalModes
        {
            YMargins,
            YMarginsFull,
            YRange
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

        private class Candle
        {
            public StockRecord Record { get; set; }
            public double X1 { get; set; }
            public double X2 { get; set; }
            public double YLow { get; set; }
            public double YHigh { get; set; }
            public double YOpen { get; set; }
            public double YClose { get; set; }
        }

        #endregion

        #region Default values

        public const VerticalModes DefaultVerticalMode = VerticalModes.YMargins;
        public const double DefaultTopMarginRatio = 0.3;
        public const double DefaultBottomMarginRatio = 0.3;
        public const double DefaultYinWidthRatio = 0.8;
        public const double DefaultYangWidthRatio = 0.8;

        #endregion

        #region Fields

        private bool _yMinMaxValueScanned;
        private double _yMaxValue;
        private double _yMinValue;

        #endregion

        /// <summary>
        ///  Creates a candel chart
        /// </summary>
        /// <param name="data">The data reference held by this drawer</param>
        /// <param name="chartWidth">The width of the chart</param>
        /// <param name="chartHeight">The height of the chart</param>
        public CandleChartPlotter()
        {
            YangWidthRatio = DefaultYangWidthRatio;
            YinWidthRatio = DefaultYinWidthRatio;
            TopMarginRatio = DefaultTopMarginRatio;
            BottomMarginRatio = DefaultBottomMarginRatio;
        }
      
        #region Chart config

        public double ChartWidth { get; set; }
        public double ChartHeight { get; set; }

        /// <summary>
        ///  Width of yang bar vs width of its allocated horizontal slot
        /// </summary>
        public double YangWidthRatio { get; set; }

        /// <summary>
        ///  Width of yin bar vs width of its allocated horizontal slot
        /// </summary>
        public double YinWidthRatio { get; set; }

        /// <summary>
        ///  Top margin vs (max-min)
        /// </summary>
        public double TopMarginRatio { get; set; }

        /// <summary>
        ///  Bottom margin vs (max-min)
        /// </summary>
        public double BottomMarginRatio { get; set; }

        /// <summary>
        ///  Value of Y on the upper boundary
        /// </summary>
        /// <remarks>
        ///  It is updated when re-drawn if the vertical mode is not YRange 
        ///  or it is specified by the user
        /// </remarks>
        public double YMax { get; set; }

        /// <summary>
        ///  Value of Y on the lower boundary
        /// </summary>
        /// <remarks>
        ///  It is updated when re-drawn if the vertical mode is not YRange 
        ///  or it is specified by the user
        /// </remarks>
        public double YMin { get; set; }

        public double YMaxValue
        {
            get
            {
                ScanMinMaxYValues();
                return _yMaxValue;
            }
        }

        public double YMinValue
        {
            get
            {
                ScanMinMaxYValues();
                return _yMinValue;
            }
        }

        #endregion

        #region Temporal
        
        public VerticalModes VertialMode { get; set; } = DefaultVerticalMode;

        #endregion

        #region Drawing delegate

        public event DrawCandleDelegate DrawCandle;

        #endregion
        
        public override void Draw(IEnumerable<StockRecord> stocks, double startSlot)
        {
            IEnumerable<Candle> candles;
            switch (VertialMode)
            {
                case VerticalModes.YMargins:
                    candles = DrawWithYMargins(stocks, startSlot);
                    break;
                case VerticalModes.YMarginsFull:
                    {
                        double ymin, ymax;
                        GetYMinMaxForMargins(YMinValue, YMaxValue, out ymin, out ymax);
                        candles = DrawWithYRange(stocks, startSlot, ymin, ymax);
                        YMin = ymin;
                        YMax = ymax;
                        break;
                    }
                case VerticalModes.YRange:
                    candles = DrawWithYRange(stocks, startSlot, YMin, YMax);
                    break;
                default:
                    throw new ArgumentException("Unexpected mode");
            }
            FireDrawBegin();
            foreach (var candle in candles)
            {
                var ct = candle.YOpen < candle.YClose ? CandleTypes.Yin : CandleTypes.Yang;
                var rectymin = Math.Min(candle.YOpen, candle.YClose);
                var rectymax = Math.Max(candle.YOpen, candle.YClose);
                var ymax = candle.YHigh;
                var ymin = candle.YLow;
                var x = (candle.X1 + candle.X2) / 2;
                if (YMode == YModes.TopToBottom)
                {
                    var temp = ChartHeight - rectymax;
                    rectymax = ChartHeight - rectymin;
                    rectymin = temp;
                    temp = ChartHeight - ymax;
                    ymax = ChartHeight - ymin;
                    ymin = temp;
                }
                var irectymin = (int)Math.Round(rectymin);
                var irectymax = (int)Math.Round(rectymax);
                var cs = new CandleShape
                {
                    Record = candle.Record,
                    CandleType = ct,
                    X = x,
                    XRectMin = candle.X1,
                    XRectMax = candle.X2,
                    YRectMin = rectymin,
                    YRectMax = rectymax,
                    YMin = ymin,
                    YMax = ymax
                };
                DrawCandle(cs);
            }
            FireDrawEnd();
        }
        
        private IEnumerable<Candle> DrawWithYRange(IEnumerable<StockRecord> records, double startSlot, double ymin, double ymax)
        {
            var yd = ymax - ymin;
            return PlotLoopYield(records, startSlot, (r, slot) =>
            {
                var low = ChartHeight * r.Low;
                var cd = new Candle
                {
                    Record = r,
                    X1 = ChartWidth * slot / Sequencer.Length,
                    X2 = ChartWidth * (slot + 1) / Sequencer.Length,
                    YOpen = (r.Open - ymin) * ChartHeight / yd,
                    YClose = (r.Close - ymin) * ChartHeight / yd,
                    YHigh = (r.High - ymin) * ChartHeight / yd,
                    YLow = (r.Low - ymin) * ChartHeight / yd
                };
                return cd;
            });
        }

        private List<Candle> DrawWithYMargins(IEnumerable<StockRecord> records, double startSlot)
        {
            var cdlist = new List<Candle>();
            var min = double.MaxValue;
            var max = double.MinValue;
            PlotLoop(records, startSlot, (r, slot) =>
            {
                var low = ChartHeight * r.Low;
                var cd = new Candle
                {
                    X1 = ChartWidth * slot / Sequencer.Length,
                    X2 = ChartWidth * (slot + 1) / Sequencer.Length,
                    // Y variables temporarily hold the original values
                    YOpen = r.Open,
                    YClose = r.Close,
                    YHigh = r.High,
                    YLow = r.Low
                };
                cdlist.Add(cd);
                if (r.Low < min) min = r.Low;
                if (r.High > max) max = r.High;
            });

            if (cdlist.Count > 0)
            {
                double ymin, ymax;
                GetYMinMaxForMargins(min, max, out ymin, out ymax);
                YMin = ymin;
                YMax = ymax;
                var yd = ymax - ymin;
                foreach (var cd in cdlist)
                {
                    cd.YOpen = (cd.YOpen - ymin) * ChartHeight / yd;
                    cd.YClose = (cd.YClose - ymin) * ChartHeight / yd;
                    cd.YHigh = (cd.YHigh - ymin) * ChartHeight / yd;
                    cd.YLow = (cd.YLow - ymin) * ChartHeight / yd;
                }
            }
            return cdlist;
        }

        private void GetYMinMaxForMargins(double min, double max, out double newMin, out double newMax)
        {
            newMax = max + (max - min) * TopMarginRatio;
            newMin = min - (max - min) * BottomMarginRatio;
        }

        private void ScanMinMaxYValues()
        {
            if (_yMinMaxValueScanned) return;
            _yMinValue = double.MaxValue;
            _yMaxValue = double.MinValue;
            foreach (var d in Sequencer.Data)
            {
                if (d.High > _yMaxValue) _yMaxValue = d.High;
                if (d.Low < _yMinValue) _yMinValue = d.Low;
            }
            _yMinMaxValueScanned = true;
        }
    }
}
