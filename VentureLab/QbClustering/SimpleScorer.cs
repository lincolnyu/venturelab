namespace VentureLab.QbClustering
{
    public class SimpleScorer : IScorer
    {
        public const double DefaultOutputPenalty = 2;

        public SimpleScorer(double inputDiffTolerance, double outputDiffNormalizer, double outputPenalty = DefaultOutputPenalty)
        {
            InputDiffTolerance = inputDiffTolerance;
            OutputDiffNormalizer = outputDiffNormalizer;
            OutputPenalty = outputPenalty;
        }

        public double InputDiffTolerance
        {
            get; private set;
        }

        public double InputDiffNormalizer => 1.0 / InputDiffTolerance;

        public double OutputDiffNormalizer
        {
            get; private set;
        }

        public double OutputPenalty { get; }

        public double Score(double inputDiff, double outputDiff)
        {
            var mdi = inputDiff * InputDiffNormalizer;
            var mdo = outputDiff * OutputDiffNormalizer;
            if (mdi > 1) return 0;
            if (mdo < 1) return 1;
            if (mdo > OutputPenalty) return -1;
            var d = (OutputPenalty + 1) / (OutputPenalty - 1);
            var k = -2.0 / (OutputPenalty + 1);
            return k * mdo + d;
        }
    }
}
    