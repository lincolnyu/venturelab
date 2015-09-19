using System;
using System.Collections.Generic;
using System.Linq;

namespace GaussianCore.Classification
{
    public static class CoreDistanceHelper
    {
        #region Methods

        public static double GetSquareDistance(this IList<ICore> cl1, IList<ICore> cl2)
        {
            if (cl2.Count < cl1.Count)
            {
                return GetSquareDistanceOrderd(cl2, cl1);
            }
            else
            {
                return GetSquareDistanceOrderd(cl1, cl2);
            }
        }

        private static double GetSquareDistanceOrderd(IList<ICore> cl1, IList<ICore> cl2)
        {
            var res = 0.0;
            foreach (var c1 in cl1)
            {
                var minD = double.MaxValue;
                double a = 0;
                foreach (var c2 in cl2)
                {
                    a = Math.Min(c1.Weight, c2.Weight);
                    var d = c1.GetNormalisedSquareDistance(c2);
                    var nd = d / a; // TODO maybe it should be other nonlinear function of d and a
                    if (d < minD)
                    {
                        minD = d;
                    }
                }
                res += minD;
            }
            var maxWeight = Math.Max(cl1.Max(x => x.Weight), cl2.Max(x => x.Weight));
            res *= maxWeight;
            res /= cl1.Count;
            return res;
        }

        /// <summary>
        ///  Returns the square distance between two cores noramlised by dividing 
        ///  the number of compoenents times the weight into it
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static double GetNormalisedSquareDistance(this ICore c1, ICore c2)
        {
            var sumNum = 0.0;
            var sumDenom = 0.0;
            var count = c1.CentersInput.Count;
            
            for (var i = 0; i < count; i++)
            {
                var d = c1.CentersInput[i] - c2.CentersInput[i];
                sumNum += d * d;
                var s = c1.CentersInput[i] + c2.CentersInput[i];
                sumDenom += s * s;
            }
            return sumNum / sumDenom;
        }

        #endregion
    }
}
