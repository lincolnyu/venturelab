using System.Collections.Generic;

namespace VentureLab.Prediction
{
    public interface IResult
    {
        IList<double> Y { get; }
        IList<double> YY { get; }
        double Strength { get; set; }
    }
}
