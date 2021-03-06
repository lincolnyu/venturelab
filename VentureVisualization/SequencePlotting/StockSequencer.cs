﻿using System;
using System.Collections.Generic;
using VentureVisualization.Samples;

namespace VentureVisualization.SequencePlotting
{
    public class StockSequencer
    {
        public delegate void DrawSequenceEventHandler(IEnumerable<ISample> samples, double startSlot);

        public delegate void PreDrawDoneEventHandler();

        public const double DefaultChartWidthToLengthRatio = 8;

        public StockSequencer(List<RecordSample> records)
        {
            Records = records;
        }

        #region Data

        public List<RecordSample> Records { get; }

        public virtual double TotalDataLength => Records.Count;

        #endregion

        public double Length { get; set; }

        public event DrawSequenceEventHandler PreDrawSequence;
        public event PreDrawDoneEventHandler PreDrawDone;
        public event DrawSequenceEventHandler DrawSequence;

        public void SetLengthByChartWidth(double chartWidth, double chartWidthToLengthRatio = DefaultChartWidthToLengthRatio)
        {
            Length = chartWidth / chartWidthToLengthRatio;
        }

        public void Draw(IEnumerable<ISample> sequence, double startSlot = 0)
        {
            PreDrawSequence?.Invoke(sequence, startSlot);
            PreDrawDone?.Invoke();
            DrawSequence(sequence, startSlot);   
        }

        public virtual IEnumerable<ISample> GetStocksStarting(int index)
        {
            for (var i = index; i < Records.Count; i++)
            {
                yield return Records[i];
            }
        }

        /// <summary>
        ///  Returns stocks at the specified times, 
        ///  returning gap record when not available; no interpolation.
        /// </summary>
        /// <param name="times">The times to return stock records for</param>
        /// <returns>The corresponding stock records</returns>
        public virtual IEnumerable<ISample> GetStocksAtTimes(IEnumerable<DateTime> times)
        {
            int? index = null;
            foreach (var time in times)
            {
                if (index == null)
                {
                    var q = new RecordSample { Date = time };
                    index = Records.BinarySearch(q);
                    if (index < 0)
                    {
                        index = -index - 1;
                        yield return new GapSample { Date = time };
                    }
                    else
                    {
                        yield return Records[index.Value];
                        index++;
                    }
                }
                else
                {
                    while (index < Records.Count && Records[index.Value].Date < time)
                    {
                        index++;
                    }
                    if (index >= Records.Count || Records[index.Value].Date > time)
                    {
                        yield return new GapSample { Date = time };
                    }
                    else // Data[index.Value].Date == time
                    {
                        yield return Records[index.Value];
                    }
                }
            }
        }
    }
}
