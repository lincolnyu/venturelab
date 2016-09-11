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
    }
}
