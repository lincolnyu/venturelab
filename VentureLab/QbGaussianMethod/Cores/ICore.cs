using System.Collections.Generic;

namespace VentureLab.QbGaussianMethod.Cores
{
    public interface ICore
    {
        double A(IList<double> x);
        double B(IList<double> x);
        IList<double> L { get; }
    }
}
