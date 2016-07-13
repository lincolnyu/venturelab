using System.Collections.Generic;

namespace VentureLab.QbClustering
{
    public interface IStrain
    {
        List<IStrainPoint> Points { get; set; }
    }
}
