using SecurityAccess.Asx;
using SecurityAccess.MultiTapMethod;
using System.Collections.Generic;

namespace SecurityAccess
{
    /// <summary>
    ///  extracts statistic point from raw stock data.
    /// </summary>
    public class Extractor
    {
        #region Methods

        /// <summary>
        ///  Sucks data and creates model
        /// </summary>
        /// <param name="dir">The directory the files are in</param>
        /// <param name="data">data in chronical order (oldest first)</param>
        /// <param name="start">days from the first one that is late enough to make 
        /// statistic point to the day to start the processing with</param>
        /// <param name="interval">interval in number of days</param>
        /// <param name="count">total number of statistic point to try to extract</param>
        public IEnumerable<StatisticPoint> Suck(IList<DailyStockEntry> data, int start, int interval, int count)
        {
            for (var i = StatisticPoint.FirstCentralDay + start; count > 0; count--)
            {
                var sp = new StatisticPoint();
                var d1 = data[i];
                sp.P1O = d1.Open;
                sp.P1H = d1.High;
                sp.P1L = d1.Low;
                sp.P1C = d1.Close;
                sp.V1 = d1.Volume;

                var d2 = data[i - 1];
                sp.P2 = d2.Close;

                var d3 = data[i - 2];
                sp.P3 = d3.Close;

                var d4 = data[i - 3];
                sp.P4 = d3.Close;

                var d5 = data[i - 4];
                var sum = d1.Close + d2.Close + d3.Close + d4.Close + d5.Close;
                sp.P5 = sum / 5;
                sp.V5 = d5.Volume;

                int j;
                for (j = 5; j < 15; j++)
                {
                    sum += data[i - j].Close;
                }
                sp.P15 = sum / 15;

                for (; j < 30; j++)
                {
                    sum += data[i - j].Close;
                }
                sp.P30 = sum / 30;

                for (; j < 90; j++)
                {
                    sum += data[i - j].Close;
                }
                sp.P90 = sum / 90;

                for (; j < 180; j++)
                {
                    sum += data[i - j].Close;
                }
                sp.P180 = sum / 180;

                for (; j < 360; j++)
                {
                    sum += data[i - j].Close;
                }
                sp.P360 = sum / 360;

                for (; j < 1800; j++)
                {
                    sum += data[i - j].Close;
                }
                sp.P1800 = sum / 1800;
                sp.FP1 = (data[i + 1].High + data[i + 1].Low) /2;
                sp.FP2 = (data[i + 2].High + data[i + 2].Low )/ 2;
                for (j = 0; j < 5; j++)
                {
                    sum += data[i + j].Close;
                }
                sp.FP5 = sum / 5;

                for (; j < 15; j++)
                {
                    sum += data[i + j].Close;
                }
                sp.FP15 = sum / 15;

                for (; j < 30; j++)
                {
                    sum += data[i + j].Close;
                }
                sp.FP30 = sum / 30;

                for (; j < 90; j++)
                {
                    sum += data[i + j].Close;
                }
                sp.FP90 = sum / 90;

                yield return sp;
            }
        }

        #endregion
    }
}
