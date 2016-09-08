using System;
using System.Collections.Generic;
using VentureCommon;

namespace VentureVisualization
{
    public abstract class SequencerSubscriber
    {
        public StockSequencer Sequencer { get; private set; }

        public abstract void Draw(IEnumerable<StockRecord> records, double startSlot = 0);

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
    }
}
