using System.Collections.Generic;
using VentureCommon;

namespace VentureVisualization
{
    public abstract class StockPlotter
    {
        public delegate void DrawBeginEndDelegate();

        protected delegate void PlotRecordDelegate(StockRecord record, double slot);

        protected delegate T PlotRecordDelegate<T>(StockRecord record, double slot);

        /// <summary>
        ///  Candle drawer to call
        /// </summary>
        public delegate void DrawShapeDelegate<T>(T shape) where T : BaseShape;

        public enum YModes
        {
            TopToBottom,
            ButtomToTop
        }

        public class BaseShape
        {
            public StockRecord Record { get; set; }
        }
        
        public const YModes DefaultYMode = YModes.TopToBottom;

        #region Data

        public StockSequencer Sequencer { get; private set; }

        #endregion

        public YModes YMode { get; set; } = DefaultYMode;

        public event DrawBeginEndDelegate DrawBegin;
        public event DrawBeginEndDelegate DrawEnd;

        #region Methods

        public abstract void Draw(IEnumerable<StockRecord> records, double startSlot);

        public void Subscribe(StockSequencer sequencer)
        {
            Sequencer = sequencer;
            Sequencer.DrawSequence += Draw;
        }

        public void Unsubscribe(StockSequencer sequencer)
        {
            Sequencer.DrawSequence -= Draw;
            Sequencer = null;
        }

        protected void PlotLoop(IEnumerable<StockRecord> records, double startSlot, PlotRecordDelegate plotRecord)
        {
            var slot = startSlot;
            foreach (var record in records)
            {
                if (slot >= Sequencer.Length)
                {
                    break;
                }
                if (!(record is StockSequencer.Gap))
                {
                    plotRecord(record, slot);
                }
                slot++;
            }
        }

        protected IEnumerable<T> PlotLoopYield<T>(IEnumerable<StockRecord> records, double startSlot, PlotRecordDelegate<T> plotRecord)
        {
            var slot = startSlot;
            foreach (var record in records)
            {
                if (slot >= Sequencer.Length)
                {
                    break;
                }
                if (!(record is StockSequencer.Gap))
                {
                    yield return plotRecord(record, slot);
                }
                slot++;
            }
        }

        protected void FireDrawBegin()
        {
            DrawBegin?.Invoke();
        }

        protected void FireDrawEnd()
        {
            DrawEnd?.Invoke();
        }

        #endregion
    }
}
