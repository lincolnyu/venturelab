using System;

namespace VentureLab.QbClustering
{
    public class StrainPair : IEquatable<StrainPair>
    {
        public StrainPair(IStrain strain1, IStrain strain2)
        {
            Strain1 = strain1;
            Strain2 = strain2;
        }
        public IStrain Strain1 { get; }
        public IStrain Strain2 { get; }

        public bool Equals(StrainPair other) => Strain1 == other.Strain1 && Strain2 == other.Strain2 || Strain1 == other.Strain2 && Strain2 == other.Strain1;

        public override bool Equals(object obj)
        {
            var otherStrain = obj as StrainPair;
            return otherStrain?.Equals(this) ?? false;
        }

        public override int GetHashCode()
        {
            var g1 = Strain1.GetHashCode();
            var g2 = Strain2.GetHashCode();
            return g2 < g1 ? HashingHelper.GetHashCodeForItems(g2, g1) : HashingHelper.GetHashCodeForItems(g1, g2);
        }
    }
}
