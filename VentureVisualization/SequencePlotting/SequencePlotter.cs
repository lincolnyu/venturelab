using System.Collections.Generic;
using VentureCommon;
using VentureVisualization.Samples;

namespace VentureVisualization.SequencePlotting
{
    public abstract class SequencePlotter : SequencerSubscriber
    {
        public delegate void DrawBeginEndDelegate();

        /// <summary>
        ///  To plot a sample
        /// </summary>
        /// <typeparam name="TSample">The type of the sample reference</typeparam>
        /// <param name="sample">The sample</param>
        /// <param name="slot">The slot where the sample is be drawn</param>
        /// <returns>True if to continue or false if to quit</returns>
        protected delegate bool PlotSampleDelegate<TSample>(ISample sample, double slot) where TSample : ISample;

        protected delegate bool PlotSampleDelegate<TSample, T>(TSample sample, double slot, out T t) where TSample : ISample;

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

        public virtual YModes YMode { get; set; } = DefaultYMode;

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
                    if (!plotSample(sample, slot))
                    {
                        break;
                    }
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
                    T t;
                    if (plotSample(sample, slot, out t))
                    {
                        yield return t;
                    }
                    else
                    {
                        break;
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
