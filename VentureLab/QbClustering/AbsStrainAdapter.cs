using System;
using System.Linq;

namespace VentureLab.QbClustering
{
    /// <summary>
    ///  Strains ordered by absolue value based rule
    /// </summary>
    public class AbsStrainAdapter : StrainAdapter
    {
        public static AbsStrainAdapter Instance { get; } = new AbsStrainAdapter();

        public override double GetPointIndicator(IStrainPoint point)
        {
            return point.Input.Sum(x => Math.Abs(x));
        }

        public override double GetInputDiff(IStrainPoint pa, IStrainPoint pb)
        {
            var diff = 0.0;
            for (var j = 0; j < pa.InputLength; j++)
            {
                var va = pa.Input[j];
                var vb = pb.Input[j];
                var d = va - vb;
                diff += Math.Abs(d);
            }
            return diff;
        }

        public override double GetOutputDiff(IStrainPoint pa, IStrainPoint pb)
        {
            var diff = 0.0;
            for (var j = 0; j < pa.OutputLength; j++)
            {
                var va = pa.Output[j];
                var vb = pb.Output[j];
                var d = va - vb;
                diff += Math.Abs(d);
            }
            return diff;
        }
    }
}
