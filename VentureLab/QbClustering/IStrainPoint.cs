using System;
using VentureLab.QbGaussianMethod.Cores;

namespace VentureLab.QbClustering
{
    public interface IStrainPoint : IPoint, IComparable<IStrainPoint>
    {
        double Indicator { get; set; }
    }
}
