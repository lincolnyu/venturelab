using System.Collections.Generic;

namespace GaussianCore.Classification
{
    /// <summary>
    ///  Compare cores by input then output
    /// </summary>
    public class CoreAlphabeticComparer : IComparer<ICore>
    {
        #region Properties

        /// <summary>
        ///  Singleton
        /// </summary>
        public static CoreAlphabeticComparer Instance { get; private set; } = new CoreAlphabeticComparer();

        #endregion

        #region Methods

        #region IComparer<ICore> members

        /// <summary>
        ///  Compares two cores first by input sequence then by output
        /// </summary>
        /// <param name="x">The first core</param>
        /// <param name="y">The second core</param>
        /// <returns>The comparison indicator</returns>
        public int Compare(ICore x, ICore y)
        {
            for (var i = 0; i < x.CentersInput.Count; i++)
            {
                var xinput = x.CentersInput[i];
                var yinput = y.CentersInput[i];
                var c = xinput.CompareTo(yinput);
                if (c != 0)
                {
                    return c;
                }
            }
            for (var i = 0; i < x.CentersOutput.Count; i++)
            {
                var xoutput = x.CentersOutput[i];
                var youtput = y.CentersOutput[i];
                var c = xoutput.CompareTo(youtput);
                if (c != 0)
                {
                    return c;
                }
            }
            return 0;
        }

        #endregion

        #endregion
    }
}
