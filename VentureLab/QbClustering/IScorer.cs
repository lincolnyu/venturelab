namespace VentureLab.QbClustering
{
    public interface IScorer
    {
        double InputDiffTolerance { get; }

        double Score(double mdi, double mdo);
    }
}
