using System;
using System.Collections.Generic;
using System.Linq;
using VentureCommon;

namespace VentureVisualization
{
    public class TimeRuler : SequencerSubscriber
    {
        public delegate void DrawDatePegDelegate(double x, DateTime dt);

        public const double DefaultMinInterval = 80;

        public TimeRuler()
        {
        }

        public double RulerWidth { get; set; }
        public double MinInterval { get; set; } = DefaultMinInterval;

        public event DrawDatePegDelegate DrawDatePeg;

        public override void Draw(IEnumerable<StockRecord> sequence, double startSlot = 0)
        {
            var len = Sequencer.Length;
            var lastX = double.MinValue;
            var slot = startSlot;
            foreach (var dt in sequence.Select(x=>x.Date))
            {
                if (slot >= Sequencer.Length)
                {
                    break;
                }
                if (dt.Day == 1 || dt.Day % 5 == 0)
                {
                    var x = slot * RulerWidth / Sequencer.Length;
                    if (x - lastX >= MinInterval)
                    {
                        DrawDatePeg(x, dt);
                        lastX = x;
                    }
                }
                slot++;
            }
        }
    }
}
