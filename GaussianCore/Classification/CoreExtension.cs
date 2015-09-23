using System.Linq;

namespace GaussianCore.Classification
{
    public class CoreExtension
    {
        #region Properties

        public ICore Core { get; set; }

        public double InputSum { get; set; }

        #endregion

        #region Methods

        public void Update()
        {
            InputSum = Core.CentersInput.Sum();
        }

        #endregion
    }
}
