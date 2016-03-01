using System.Collections.Generic;

namespace GaussianCore
{
    /// <summary>
    ///  A core with input and output centres
    /// </summary>
    public interface ICore
    {
        #region Properties

        /// <summary>
        ///  centers of components of input
        /// </summary>
        IList<double> CentersInput { get; }

        /// <summary>
        ///  centers of components of output
        /// </summary>
        IList<double> CentersOutput { get; }

        /// <summary>
        ///  A factor characterises the significance of the core comparative to other cores
        /// </summary>
        double Weight { get; }

        #endregion
    }
}
