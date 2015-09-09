using System;
using System.Collections.Generic;

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
                foreach (var c2 in cl2)
                {
                    var d = c1.GetSquareDistance(c2);
                    if (d < minD)
                    {
                        minD = d;
                    }
                }
                res += minD;
            }
            return res;
        }

        public static double GetSquareDistance(this ICore c1, ICore c2)
        {
            var sum = 0.0;
            for (var i = 0; i < c1.CentersInput.Count; i++)
            {
                var d = c1.CentersInput[i] - c2.CentersInput[i];
                sum += d * d;
            }
            var a = Math.Min(c1.Weight, c2.Weight);
            return sum * 2;
        }

        #endregion
    }
}
