using System;
using System.Collections.Generic;
using VentureCommon;

namespace VentureVisualization
{
    public class CandleChartPlotter
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

        public enum YModes
        {
            TopToBottom,
            ButtomToTop
        }

        public class CandleShape
        {
            public StockRecord Record { get; set; }
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

        private class Gap : StockRecord
        {
            public static readonly Gap Instance = new Gap();
        }

        #endregion

        #region Default values

        public const VerticalModes DefaultVerticalMode = VerticalModes.YMargins;
        public const YModes DefaultYMode = YModes.TopToBottom;
        public const double DefaultChartWidthToLengthRatio = 8;
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

        public delegate void DrawBeginEndDelegate();

        /// <summary>
        ///  Candle drawer to call
        /// </summary>
        public delegate void DrawCandleDelegate(CandleShape shape);

        /// <summary>
        ///  Creates a candel chart
        /// </summary>
        /// <param name="data">The data reference held by this drawer</param>
        /// <param name="chartWidth">The width of the chart</param>
        /// <param name="chartHeight">The height of the chart</param>
        public CandleChartPlotter(List<StockRecord> data, 
            double chartWidth, double chartHeight, 
            DrawBeginEndDelegate drawBegin,
            DrawCandleDelegate drawCandle, 
            DrawBeginEndDelegate drawEnd)
        {
            Data = data;
            ChartWidth = chartWidth;
            ChartHeight = chartHeight;
            YangWidthRatio = DefaultYangWidthRatio;
            YinWidthRatio = DefaultYinWidthRatio;
            TopMarginRatio = DefaultTopMarginRatio;
            BottomMarginRatio = DefaultBottomMarginRatio;
            Length = ChartWidth / DefaultChartWidthToLengthRatio;
            DrawBegin = drawBegin;
            DrawEnd = drawEnd;
            DrawCandle = drawCandle;
        }

        #region Data

        public List<StockRecord> Data { get; }

        #endregion

        #region Chart config

        public double ChartWidth { get; set; }
        public double ChartHeight { get; set; }

        public YModes YMode { get; set; } = DefaultYMode;

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

        public double YMax { get; set; }
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

        public int StartIndex
        {
            get; set;
        }

        public double Length { get; set; }

        public VerticalModes VertialMode { get; set; } = DefaultVerticalMode;

        #endregion

        #region Drawing delegate

        DrawCandleDelegate DrawCandle { get; }
        DrawBeginEndDelegate DrawBegin { get; }
        DrawBeginEndDelegate DrawEnd { get; }  

        #endregion

        public void Draw(IEnumerable<DateTime> times, double startSlot = 0)
        {
            var stocks = GetStocksAtTimes(times);
            DrawCandles(stocks, startSlot);
        }

        public void Draw(int stockIndex, double startSlot = 0)
        {
            var stocks = GetStocksStarting(stockIndex);
            DrawCandles(stocks, startSlot);
        }

        private void DrawCandles(IEnumerable<StockRecord> stocks, double startIndex)
        {
            IEnumerable<Candle> candles;
            switch (VertialMode)
            {
                case VerticalModes.YMargins:
                    candles = DrawWithYMargins(stocks, startIndex);
                    break;
                case VerticalModes.YMarginsFull:
                    {
                        double ymin, ymax;
                        GetYMinMaxForMargins(YMinValue, YMaxValue, out ymin, out ymax);
                        candles = DrawWithYRange(stocks, startIndex, ymin, ymax);
                        break;
                    }
                case VerticalModes.YRange:
                    candles = DrawWithYRange(stocks, startIndex, YMin, YMax);
                    break;
                default:
                    throw new ArgumentException("Unexpected mode");
            }
            DrawBegin();
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
            DrawEnd();
        }

        /// <summary>
        ///  Returns stocks at the specified times, 
        ///  returning gap record when not available; no interpolation.
        /// </summary>
        /// <param name="times">The times to return stock records for</param>
        /// <returns>The corresponding stock records</returns>
        private IEnumerable<StockRecord> GetStocksAtTimes(IEnumerable<DateTime> times)
        {
            int? index = null;
            foreach (var time in times)
            {
                if (index == null)
                {
                    var q = new StockRecord { Date = time };
                    index = Data.BinarySearch(q);
                    if (index < 0)
                    {
                        index = -index - 1;
                        yield return Gap.Instance;
                    }
                    else
                    {
                        yield return Data[index.Value];
                        index++;
                    }
                }
                else
                {
                    while (index < Data.Count && Data[index.Value].Date < time)
                    {
                        index++;
                    }
                    if (index >= Data.Count || Data[index.Value].Date > time)
                    {
                        yield return Gap.Instance;
                    }
                    else // Data[index.Value].Date == time
                    {
                        yield return Data[index.Value];
                    }
                }
            }
        }

        private IEnumerable<StockRecord> GetStocksStarting(int index)
        {
            for (var i = index; i < Data.Count; i++)
            {
                yield return Data[i];
            }
        }

        private IEnumerable<Candle> DrawWithYRange(IEnumerable<StockRecord> records, double startSlot, double ymin, double ymax)
        {
            var slot = startSlot;
            var yd = ymax - ymin;
            foreach (var r in records)
            {
                var low = ChartHeight * r.Low;
                var cd = new Candle
                {
                    Record = r,
                    X1 = ChartWidth * slot / Length,
                    X2 = ChartWidth * (slot + 1) / Length,
                    YOpen = (r.Open - ymin) * ChartHeight / yd,
                    YClose = (r.Close - ymin) * ChartHeight / yd,
                    YHigh = (r.High - ymin) * ChartHeight / yd,
                    YLow = (r.Low - ymin) * ChartHeight / yd
                };
                yield return cd;
                slot++;
            }
        }

        private List<Candle> DrawWithYMargins(IEnumerable<StockRecord> records, double startSlot)
        {
            var slot = startSlot;
            var cdlist = new List<Candle>();
            var min = double.MaxValue;
            var max = double.MinValue;
            foreach (var r in records)
            {
                if (slot >= Length)
                {
                    break;
                }
                var low = ChartHeight * r.Low;
                var cd = new Candle
                {
                    X1 = ChartWidth * slot / Length,
                    X2 = ChartWidth * (slot + 1) / Length,
                    // Y variables temporarily hold the original values
                    YOpen = r.Open,
                    YClose = r.Close,
                    YHigh = r.High,
                    YLow = r.Low
                };
                cdlist.Add(cd);
                if (r.Low < min) min = r.Low;
                if (r.High > max) max = r.High;
                slot++;
            }

            if (cdlist.Count > 0)
            {
                double ymin, ymax;
                GetYMinMaxForMargins(min, max, out ymin, out ymax);
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
            foreach (var d in Data)
            {
                if (d.High > _yMaxValue) _yMaxValue = d.High;
                if (d.Low < _yMinValue) _yMinValue = d.Low;
            }
            _yMinMaxValueScanned = true;
        }
    }
}
