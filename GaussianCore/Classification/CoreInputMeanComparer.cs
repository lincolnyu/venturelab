using System.Collections.Generic;

namespace GaussianCore.Classification
{
    /// <summary>
    ///  Compares two cores based on sum (equivalently mean) of inputs
    /// </summary>
    public class CoreInputMeanComparer : IComparer<CoreExtension>
    {
        #region Properties

        /// <summary>
        ///  Singleton
        /// </summary>
        public static CoreInputMeanComparer Instance { get; private set; } = new CoreInputMeanComparer();

        #endregion

        #region Methods

        #region IComparer<CoreExtension> members

        /// <summary>
        ///  Compares two cores by the sums of their inputs
        /// </summary>
        /// <param name="x">The first (extended) core</param>
        /// <param name="y">The second (extended) core</param>
        /// <returns>The comparison indicator</returns>
        public int Compare(CoreExtension x, CoreExtension y)
        {
            return x.InputSum.CompareTo(y.InputSum);
        }

        #endregion

        #endregion
    }
}
