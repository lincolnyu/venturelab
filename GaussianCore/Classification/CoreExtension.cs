using System.Linq;

namespace GaussianCore.Classification
{
    /// <summary>
    ///  Encapsulates core and some extra info
    /// </summary>
    public class CoreExtension
    {
        #region Properties

        /// <summary>
        ///  The encapsulated core
        /// </summary>
        public ICore Core { get; set; }

        /// <summary>
        ///  Sum of input for indexing 
        /// </summary>
        public double InputSum { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///  Update the property 'InputSum'
        /// </summary>
        public void Update()
        {
            InputSum = Core.CentersInput.Sum();
        }

        #endregion
    }
}
