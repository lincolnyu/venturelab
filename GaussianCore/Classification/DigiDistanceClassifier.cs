using System.Collections.Generic;

namespace GaussianCore.Classification
{
    public class DigiDistanceClassifier
    {
        #region Nested types

        public class CodeCoreList
        {
            public string Code { get; set; }

            public List<ICore> Cores { get; private set; } = new List<ICore>();

            public void SortCores()
            {
                Cores.Sort(CoreAlphabeticComparer.Instance);
            }
        }

        #endregion

        #region Properties

        public double[] MatchThreshold { get; set; }

        #endregion

        #region Methods

        public void Load(IList<CodeCoreList> codeCoreLists)
        {

        }

        public static int Search (ICore core, IList<ICore> list, double[] matchThr)
        {

        }

        #endregion
    }
}
