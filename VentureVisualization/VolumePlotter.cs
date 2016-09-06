using System;
using System.Collections.Generic;
using System.Linq;
using VentureCommon;

namespace VentureVisualization
{
    using DrawVolumeDelegate = BasePlotter.DrawShapeDelegate<VolumePlotter.VolumeShape>;

    public sealed class VolumePlotter : BasePlotter
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

        public VolumePlotter(DrawBeginEndDelegate drawBegin,
            DrawVolumeDelegate drawVolume, DrawBeginEndDelegate drawEnd) : base(drawBegin, drawEnd)
        {
            DrawVolume = drawVolume;
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

        private DrawVolumeDelegate DrawVolume { get; }

        public override void Draw(IEnumerable<StockRecord> records, double startSlot)
        {
            double ymax;
            switch (VertialMode)
            {
                case VerticalModes.YVisible:
                    ymax = records.Max(x => x.Volume);
                    break;
                case VerticalModes.YFull:
                    ymax = MaxVolume;
                    break;
                default:
                    throw new ArgumentException("Unexpected vertical mode");
            }
            DrawBegin();
            PlotLoop(records, startSlot, (record, slot) =>
            {
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
            });
            DrawEnd();
        }
        
        private void ScanMaxVolume()
        {
            if (_maxVolumeScanned) return;
            _maxVolume = double.MinValue;
            foreach (var d in Sequencer.Data)
            {
                if (d.Volume > _maxVolume) _maxVolume = d.Volume;
            }
            _maxVolumeScanned = true;
        }
    }
}
