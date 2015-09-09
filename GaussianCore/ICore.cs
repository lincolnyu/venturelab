using System.Collections.Generic;

namespace GaussianCore
{
    /// <summary>
    ///  A core with input and output centres
    /// </summary>
    public interface ICore
    {
        #region Properties

        IList<double> CentersInput { get; }

        IList<double> CentersOutput { get; }

        double Weight { get; }

        #endregion
    }
}
