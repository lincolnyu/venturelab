using System.Collections.Generic;

namespace VentureLab.QbGaussianMethod.Cores
{
    public interface ICoreFactory
    {
        IEnumerable<ICore> CreateCores(IEnumerable<IPoint> points);
    }
}
