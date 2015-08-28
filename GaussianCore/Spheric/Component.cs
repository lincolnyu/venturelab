namespace GaussianCore.Spheric
{
    public class Component
    {
        #region Properties

        public double Center { get; set; }

        /// <summary>
        ///          (x-x0)^2                  1
        /// exp( -  ----------- ), L = -  -----------
        ///          2*sigma^2             2*sigma^2
        /// </summary>
        public double L { get; set; }

        #endregion
    }
}
