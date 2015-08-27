using System.Collections.Generic;

namespace GaussianCore
{
    public class ConfinedCoreManager : GenericCoreManager
    {
        #region Properties

        public IList<GaussianConfinedCore> Cores { get; } = new List<GaussianConfinedCore>();

        #endregion

        #region Methods

        #region GenericCoreManager members

        public override IEnumerator<GenericCore> GetEnumerator()
        {
            foreach (var c in Cores)
            {
                yield return c;
            }
        }

        #endregion

        public void UpdateCoreCoeffs()
        {

        }

        #endregion
    }
}
