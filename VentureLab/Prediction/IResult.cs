using System.Collections.Generic;

namespace VentureLab.Prediction
{
    public interface IResult
    {
        IList<double> Y { get; }
        IList<double> YY { get; }
        /// <summary>
        ///  Confidence based on certain criteria typically the 
        ///  a priori probability
        /// </summary>
        double Strength { get; set; }
    }
}
