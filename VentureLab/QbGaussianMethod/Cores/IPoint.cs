using System.Collections.Generic;

namespace VentureLab.QbGaussianMethod.Cores
{
    public interface IPoint
    {
        IList<double> Input { get; }
        IList<double> Output { get; }

        int InputLength { get; }
        int OutputLength { get; }
    }
}
