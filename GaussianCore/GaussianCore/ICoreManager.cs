using System.Collections.Generic;

namespace GaussianCore
{
    public interface ICoreManager : IEnumerable<Core>
    {
        #region Methods

        double GetIntensity(IList<double> inoutputs);

        double GetExpectedY(IList<double> inputs, int k);

        double GetExpectedSquareY(IList<double> inputs, int k);

        #endregion
    }
}
