using System;
using System.Linq;

namespace VentureLab.QbClustering
{
    /// <summary>
    ///  Strains ordered by square rule 
    /// </summary>
    public class SqrStrainAdapter : StrainAdapter
    {
        public static SqrStrainAdapter Instance { get; } = new SqrStrainAdapter();

        public override double GetPointIndicator(IStrainPoint point)
        {
            return Math.Sqrt(point.Input.Sum(x => x * x));
        }

        public override double GetInputDiff(IStrainPoint pa, IStrainPoint pb)
        {
            var diff = 0.0;
            for (var i = 0; i < pa.InputLength; i++)
            {
                var va = pa.Input[i];
                var vb = pb.Input[i];
                var d = va - vb;
                diff += d * d;
            }
            return Math.Sqrt(diff);
        }

        public override double GetOutputDiff(IStrainPoint pa, IStrainPoint pb)
        {
            var diff = 0.0;
            for (var i = 0; i < pa.OutputLength; i++)
            {
                var va = pa.Output[i];
                var vb = pb.Output[i];
                var d = va - vb;
                diff += d * d;
            }
            return Math.Sqrt(diff);
        }
    }
}
