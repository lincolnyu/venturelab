using System;
using System.Collections.Generic;
using VentureVisualization.Samples;

namespace VentureVisualization.SequencePlotting
{
    public class TimeRuler : SequencePlotter
    {
        public class TimeShape : IComparable<TimeShape>
        {
            public double X { get; set; }

            public int CompareTo(TimeShape other) => X.CompareTo(other.X);
        }

        public class PastDateShape : TimeShape
        {
            public IDatedSample Sample { get; set; }
            public DateTime Date => Sample.Date;

            public override string ToString() => $"{Date:dd/MM/yy}";
        }

        public class FutureDateShape : TimeShape
        {
            public IFutureSample Sample { get; set; }
            public int Days => Sample.Days;

            public override string ToString() => $"{Days}d";
        }

        public class FutureDateSamplessShape : TimeShape
        {
            public int Days { get; set; }

            public override string ToString() => $"{Days}d";
        }

        private List<TimeShape> _timeShapeCache = new List<TimeShape>();

        public delegate void DrawDatePegDelegate(TimeShape ts);

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
            _timeShapeCache.Clear();

            PlotLoop(sequence, startSlot, (s, slot) =>
            {
                slot += 0.5;
                var dts = s as IDatedSample;
                if (dts != null && DrawDatePeg != null)
                {
                    var dt = dts.Date;
                    var x = slot * RulerWidth / Sequencer.Length;
                    var ds = new PastDateShape { X = x, Sample = dts };
                    if (dt.Day == 1 || dt.Day % 5 == 0)
                    {
                        if (x - lastX >= MinInterval)
                        {
                            DrawDatePeg(ds);
                            lastX = x;
                        }
                    }
                    AddToCache(ds);
                    return true;
                }
                var fs = s as IFutureSample;
                if (fs != null)
                {
                    var x = slot * RulerWidth / Sequencer.Length;
                    var fds = new FutureDateShape { X = x, Sample = fs };
                    if (x - lastX >= MinInterval)
                    {
                        DrawDatePeg(fds);
                        lastX = x;
                    }
                    AddToCache(fds);
                }
                return true;
            });
        }

        private void AddToCache(TimeShape ts)
        {
            var index = _timeShapeCache.BinarySearch(ts);
            if (index < 0) index = -index - 1;
            _timeShapeCache.Insert(index, ts);
        }

        public TimeShape FromXToTimeShape(double x)
        {
            var q = new TimeShape { X = x };
            var index = _timeShapeCache.BinarySearch(q);
            if (index < 0) index = -index - 1;
            if (index >= _timeShapeCache.Count) return null;
            if (index > 0)
            {
                var t1 = _timeShapeCache[index - 1];
                var t2 = _timeShapeCache[index];
                var d1 = x - t1.X;
                var d2 = t2.X - x;
                
                var t1f = t1 as FutureDateShape;
                var t2f = t2 as FutureDateShape;
                if (t1f != null & t2f != null)
                {
                    var interp = (int)Math.Round((t1f.Days * d2 + t2f.Days * d1) / (d1 + d2));
                    return new FutureDateSamplessShape { Days = interp, X = x };
                }
                return d1 < d2 ? t1 : t2;
            }
            else
            {
                return _timeShapeCache[index];
            }
        }
    }
}
