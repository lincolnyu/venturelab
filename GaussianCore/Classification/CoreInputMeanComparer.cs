using System.Collections.Generic;

namespace GaussianCore.Classification
{
    public class CoreInputMeanComparer : IComparer<CoreExtension>
    {
        #region Properties

        public static CoreInputMeanComparer Instance { get; private set; } = new CoreInputMeanComparer();

        #endregion

        #region Methods

        #region IComparer<CoreExtension> members

        public int Compare(CoreExtension x, CoreExtension y)
        {
            return x.InputSum.CompareTo(y.InputSum);
        }

        #endregion

        #endregion
    }
}
