using System.Collections.Generic;
using VentureCommon;
using VentureVisualization.Samples;

namespace VentureVisualization.SequencePlotting
{
    public abstract class SequencePlotter : SequencerSubscriber
    {
        public delegate void DrawBeginEndDelegate();

        protected delegate void PlotSampleDelegate<TSample>(ISample record, double slot) where TSample : ISample;

        protected delegate T PlotSampleDelegate<TSample, T>(TSample record, double slot) where TSample : ISample;

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

        public SequencePlotter(bool subscribePreDraw) : base(subscribePreDraw)
        {
        }

        public YModes YMode { get; set; } = DefaultYMode;

        public event DrawBeginEndDelegate DrawBegin;
        public event DrawBeginEndDelegate DrawEnd;

        #region Methods

        protected void PlotLoop<TSample>(IEnumerable<TSample> samples, double startSlot, PlotSampleDelegate<TSample> plotSample) where TSample : ISample
        {
            var slot = startSlot;
            foreach (var sample in samples)
            {
                if (slot >= Sequencer.Length)
                {
                    break;
                }
                if (!(sample is GapSample))
                {
                    plotSample(sample, slot);
                }
                slot += sample.Step;
            }
        }

        protected IEnumerable<T> PlotLoopYield<TSample, T>(IEnumerable<TSample> samples, double startSlot, PlotSampleDelegate<TSample, T> plotSample) where TSample : ISample
        {
            var slot = startSlot;
            foreach (var sample in samples)
            {
                if (slot >= Sequencer.Length)
                {
                    break;
                }
                if (!(sample is GapSample))
                {
                    var v = plotSample(sample, slot);
                    if (v != null)
                    {
                        yield return v;
                    }
                }
                slot += sample.Step;
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
