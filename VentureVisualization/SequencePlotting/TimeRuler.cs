using System;
using System.Collections.Generic;
using VentureVisualization.Samples;

namespace VentureVisualization.SequencePlotting
{
    public class TimeRuler : SequencePlotter
    {
        public delegate void DrawDatePegDelegate(double x, DateTime dt);
        public delegate void DrawFutureDatePegDelegate(double x, int days);

        public const double DefaultMinInterval = 80;

        public TimeRuler() : base(false)
        {
        }

        public double RulerWidth { get; set; }
        public double MinInterval { get; set; } = DefaultMinInterval;

        public event DrawDatePegDelegate DrawDatePeg;
        public event DrawFutureDatePegDelegate DrawFutureDatePeg;

        public override void Draw(IEnumerable<ISample> sequence, double startSlot = 0)
        {
            var len = Sequencer.Length;
            var lastX = double.MinValue;

            PlotLoop(sequence, startSlot, (s, slot) =>
            {
                slot += 0.5;
                var dts = s as IDatedSample;
                if (dts != null && DrawDatePeg != null)
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
                    return true;
                }
                var fs = s as IFutureSample;
                if (fs != null && DrawFutureDatePeg != null)
                {
                    var x = slot * RulerWidth / Sequencer.Length;
                    DrawFutureDatePeg(x, fs.Days);
                }
                return true;
            });
        }
    }
}
