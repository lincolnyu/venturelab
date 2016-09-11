using System;
using System.Collections.Generic;
using System.Linq;
using VentureCommon;

namespace VentureVisualization.SequencePlotting
{
    using Samples;
    using DrawVolumeDelegate = SequencePlotter.DrawShapeDelegate<VolumePlotter.VolumeShape>;

    public sealed class VolumePlotter : SamplePlotter
    {
        public enum VerticalModes
        {
            YVisible,
            YFull,
        }

        public class VolumeShape : BaseShape
        {
            public double X { get; set; }
            public double Y { get; set; }
        }

        public const VerticalModes DefaultVerticalMode = VerticalModes.YFull;
        private double _maxVolume;
        private bool _maxVolumeScanned;

        public VolumePlotter() : base(false)
        {
        }

        public double ChartWidth { get; set; }
        public double ChartHeight { get; set; }

        public VerticalModes VertialMode { get; set; } = DefaultVerticalMode;

        public double MaxVolume
        {
            get
            {
                ScanMaxVolume();
                return _maxVolume;
            }
        }

        public event DrawVolumeDelegate DrawVolume;

        public override void Draw(IEnumerable<VentureVisualization.ISample> samples, double startSlot)
        {
            double ymax;
            switch (VertialMode)
            {
                case VerticalModes.YVisible:
                    ymax = samples.OfType<StockRecord>().Max(x => x.Volume);
                    break;
                case VerticalModes.YFull:
                    ymax = MaxVolume;
                    break;
                default:
                    throw new ArgumentException("Unexpected vertical mode");
            }
            FireDrawBegin();
            PlotLoop(samples, startSlot, (s, slot) =>
            {
                var record = s as RecordSample;
                if (record == null) return true;
                var y = record.Volume * ChartHeight / ymax;
                if (YMode == YModes.TopToBottom)
                {
                    y = ChartHeight - y;
                }
                var vs = new VolumeShape
                {
                    X = ChartWidth * (slot + 0.5) / Sequencer.Length,
                    Y = y
                };
                DrawVolume(vs);
                return true;
            });
            FireDrawEnd();
        }
        
        private void ScanMaxVolume()
        {
            if (_maxVolumeScanned) return;
            _maxVolume = double.MinValue;
            foreach (var d in Sequencer.Records)
            {
                if (d.Volume > _maxVolume) _maxVolume = d.Volume;
            }
            _maxVolumeScanned = true;
        }
    }
}
