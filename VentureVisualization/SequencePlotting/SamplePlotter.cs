namespace VentureVisualization.SequencePlotting
{
    public abstract class SamplePlotter : SequencePlotter
    {
        public enum YModes
        {
            TopToBottom,
            ButtomToTop
        }

        public const YModes DefaultYMode = YModes.TopToBottom;

        protected SamplePlotter(bool subscribePreDraw) : base(subscribePreDraw)
        {
        }

        public virtual YModes YMode { get; set; } = DefaultYMode;

        public virtual double ChartWidth { get; set; }
        public virtual double ChartHeight { get; set; }

        public double YToViewY(double y, YMarginManager manager)
        {
            var d = manager.YMax - manager.YMin;
            var viewY = (y - manager.YMin) * ChartHeight / d;
            if (YMode == YModes.TopToBottom) viewY = ChartHeight - viewY;
            return viewY;
        }

        public double ViewYToY(double viewY, YMarginManager manager)
        {
            var d = manager.YMax - manager.YMin;
            if (YMode == YModes.TopToBottom) viewY = ChartHeight - viewY;
            var y = viewY * d / ChartHeight + manager.YMin;
            return y;
        }
    }
}
