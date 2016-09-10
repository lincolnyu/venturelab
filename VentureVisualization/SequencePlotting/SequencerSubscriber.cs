using System.Collections.Generic;

namespace VentureVisualization.SequencePlotting
{
    public abstract class SequencerSubscriber
    {
        private bool _subscribePreDraw;

        protected SequencerSubscriber(bool subscribePreDraw)
        {
            _subscribePreDraw = subscribePreDraw;
        }

        public StockSequencer Sequencer { get; private set; }

        public virtual void PreDraw(IEnumerable<ISample> samples, double startSlot = 0)
        {
        }

        public abstract void Draw(IEnumerable<ISample> samples, double startSlot = 0);

        public void Subscribe(StockSequencer sequencer)
        {
            Sequencer = sequencer;
            Sequencer.DrawSequence += Draw;
            if (_subscribePreDraw)
            {
                Sequencer.PreDrawSequence += PreDraw;
            }
        }

        public void Unsubscribe(StockSequencer sequencer)
        {
            if (_subscribePreDraw)
            {
                Sequencer.PreDrawSequence -= PreDraw;
            }
            Sequencer.DrawSequence -= Draw;
            Sequencer = null;
        }
    }
}
