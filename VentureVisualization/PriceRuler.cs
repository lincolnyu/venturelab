using System;

namespace VentureVisualization
{
    public class PriceRuler
    {
        public delegate void DrawMajorDelegate(double y, double value);

        public const double DefaultMajorThrRatio = 30;
        public const double DefaultMinMajorCount = 2;

        public PriceRuler(CandleChartPlotter candleChartPlotter)
        {
            CandleChartPlotter = candleChartPlotter;
        }

        public CandleChartPlotter CandleChartPlotter { get; }

        public double RulerWidth { get; set; }
        public double RulerHeight => CandleChartPlotter.ChartHeight;

        public double MaxMajorRatio { get; set; } = DefaultMajorThrRatio;
        /// <summary>
        ///  It has to be a value smaller than 10
        /// </summary>
        public double MinMajorCount { get; set; } = DefaultMinMajorCount;

        public event DrawMajorDelegate DrawMajor;

        public void Draw()
        {
            var max = CandleChartPlotter.YMax;
            var min = CandleChartPlotter.YMin;
            var d = max - min;
            var maxMajors = RulerHeight / MaxMajorRatio;
            double interval, start;
            GetMajorInterval(min, max, maxMajors, out interval, out start);
            for (var bar = start; bar < max; bar += interval)
            {
                var y = (bar - min) * RulerHeight / d;
                if (CandleChartPlotter.YMode == StockPlotter.YModes.TopToBottom)
                {
                    y = RulerHeight - y;
                    DrawMajor(y, bar);
                }
            }
        }


        private void GetMajorInterval(double min, double max, 
           double maxMajors, out double interval, out double start)
        {
            var d = max - min;
            var p = (int)Math.Floor(Math.Log10(d));
            var d0 = Math.Pow(10, p);
            var n = d / d0; // < 10
            if (n * 10 < maxMajors)
            {
                var dp = (int)Math.Floor(Math.Log10(maxMajors / n));
                p -= dp;
                d0 = Math.Pow(10, p);
                n = d / d0; // < 10
            }
            else if (n < MinMajorCount && MinMajorCount < maxMajors)
            {
                var a = Math.Ceiling(MinMajorCount / n); // a < 10
                if (a < 2)
                {
                    d0 *= 0.5;
                }
                else
                {
                    d0 *= 0.25;
                }
                n = d / d0;
            }
            interval = d0;
            var istart = (int)Math.Ceiling(min / interval);
            start = istart * interval;
        }
    }
}
