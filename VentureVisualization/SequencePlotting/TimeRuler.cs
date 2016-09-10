using System;
using System.Collections.Generic;
using VentureVisualization.Samples;

namespace VentureVisualization.SequencePlotting
{
    public class TimeRuler : SequencerSubscriber
    {
        public delegate void DrawDatePegDelegate(double x, DateTime dt);

        public const double DefaultMinInterval = 80;

        public TimeRuler() : base(false)
        {
        }

        public double RulerWidth { get; set; }
        public double MinInterval { get; set; } = DefaultMinInterval;

        public event DrawDatePegDelegate DrawDatePeg;

        public override void Draw(IEnumerable<ISample> sequence, double startSlot = 0)
        {
            var len = Sequencer.Length;
            var lastX = double.MinValue;
            var slot = startSlot;
            foreach (var s in sequence)
            {
                if (slot >= Sequencer.Length)
                {
                    break;
                }
                var dts = s as IDatedSample;
                if (dts != null)
                {
                    var dt = dts.Date;
                    if (dt.Day == 1 || dt.Day % 5 == 0)
                    {
                        var x = slot * RulerWidth / Sequencer.Length;
                        if (x - lastX >= MinInterval)
                        {
                            DrawDatePeg(x, dt);
                            lastX = x;
                        }
                    }
                }
                slot++;
            }
        }
    }
}
