using System.Collections.Generic;

namespace VentureLab.QbGaussianMethod.Cores
{
    public interface ICore
    {
        double A(IList<double> x);
        double B(IList<double> x);
        IList<double> L { get; }

        // integral of C(x) over entire domain
        double Integral { get; }

        // Max C(x) over x
        // to get max normalized C(x), simply divide this number by Integral
        double MaxCx { get; }
    }
}
