namespace VentureLab.QbGaussianMethod.Cores
{
    public class GaussianRegulatedCoreConstants
    {
        public GaussianRegulatedCoreConstants(int outputLength, double m, double n)
        {
            OutputLength = outputLength;
            M = m;
            N = n;
        }

        public IPoint Owner { get; }

        public double M { get; }

        public double N { get; }

        public int OutputLength { get; }

        public double P => M - N * OutputLength / 2.0;
    }
}
