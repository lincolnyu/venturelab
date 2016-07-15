using System.Collections.Generic;

namespace VentureLab.QbClustering
{
    public class Strain : IStrain
    {
        public List<IStrainPoint> Points { get; set; }
    }
}
