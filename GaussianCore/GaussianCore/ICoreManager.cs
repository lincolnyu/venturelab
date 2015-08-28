using System.Collections.Generic;

namespace GaussianCore
{
    public interface ICoreManager
    {
        #region Methods

        double GetIntensity(IList<double> inputs, IList<double> outputs);

        double GetExpectedY(IList<double> inputs, int k);

        double GetExpectedSquareY(IList<double> inputs, int k);

        #endregion
    }
}
