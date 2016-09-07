using System;
using System.Collections.Generic;
using System.Linq;
using VentureCommon;

namespace VentureVisualization
{
    using DrawVolumeDelegate = StockPlotter.DrawShapeDelegate<VolumePlotter.VolumeShape>;

    public sealed class VolumePlotter : StockPlotter
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

        public VolumePlotter() 
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
            FireDrawBegin();
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
            FireDrawEnd();
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
