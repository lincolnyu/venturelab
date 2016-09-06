using System;
using System.Collections.Generic;
using VentureCommon;

namespace VentureVisualization
{
    public class StockSequencer
    {
        public delegate void DrawSequenceEventHandler(IEnumerable<StockRecord> records, double startSlot);

        public class Gap : StockRecord
        {
            public static readonly Gap Instance = new Gap();
        }

        public const double DefaultChartWidthToLengthRatio = 8;

        public StockSequencer(List<StockRecord> data)
        {
            Data = data;
        }

        #region Data

        public List<StockRecord> Data { get; }

        #endregion

        public double Length { get; set; }

        public event DrawSequenceEventHandler DrawSequence;

        public void SetLengthByChartWidth(double chartWidth, double chartWidthToLengthRatio = DefaultChartWidthToLengthRatio)
        {
            Length = chartWidth / chartWidthToLengthRatio;
        }

        public void Draw(IEnumerable<StockRecord> sequence, double startSlot = 0)
        {
            DrawSequence(sequence, startSlot);   
        }

        public IEnumerable<StockRecord> GetStocksStarting(int index)
        {
            for (var i = index; i < Data.Count; i++)
            {
                yield return Data[i];
            }
        }

        /// <summary>
        ///  Returns stocks at the specified times, 
        ///  returning gap record when not available; no interpolation.
        /// </summary>
        /// <param name="times">The times to return stock records for</param>
        /// <returns>The corresponding stock records</returns>
        public IEnumerable<StockRecord> GetStocksAtTimes(IEnumerable<DateTime> times)
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
    }
}
