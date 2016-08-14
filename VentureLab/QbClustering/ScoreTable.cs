using System.Collections.Generic;

namespace VentureLab.QbClustering
{
    public class ScoreTable
    {
        public ScoreTable()
        {
        }

        public double this[StrainPair key]
        {
            get
            {
                return Data[key];
            }
            set
            {
                Data[key] = value;
            }
        }

        public double this[IStrain a, IStrain b]
        {
            get
            {
                var sp = new StrainPair(a, b);
                var val = 0.0;
                Data.TryGetValue(sp, out val);
                return val;
            }
            set
            {
                var sp = new StrainPair(a, b);
                Data[sp] = value;
            }
        }

        public Dictionary<StrainPair, double> Data { get; } = new Dictionary<StrainPair, double>();

        public bool TryGetValue(IStrain a, IStrain b, out double value) => TryGetValue(new StrainPair(a, b), out value);

        public bool TryGetValue(StrainPair key, out double value) => Data.TryGetValue(key, out value);
    }
}
