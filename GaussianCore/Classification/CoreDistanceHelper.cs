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
                return GetDistanceSmallListFirst(cl2, cl1);
            }
            else
            {
                return GetDistanceSmallListFirst(cl1, cl2);
            }
        }
        
        /// <summary>
        ///  
        /// </summary>
        /// <param name="cl1"></param>
        /// <param name="cl2"></param>
        /// <returns></returns>
        private static double GetDistanceSmallListFirst(IList<ICore> cl1, IList<ICore> cl2)
        {
            var res = 0.0;
            foreach (var c1 in cl1)
            {
                var minD = double.MaxValue;
                foreach (var c2 in cl2)
                {
                    var w = Math.Min(c1.Weight, c2.Weight);
                    var threshold = minD * w;
                    var d = c1.GetQuanbenDistance(c2, threshold);
                    if (d < threshold)
                    {
                        // TODO maybe it should be other nonlinear function of d and a
                        minD = d/w;
                    }
                }
                res += minD;
            }
            // normalise about weight
            var maxWeight = Math.Max(cl1.Max(x => x.Weight), cl2.Max(x => x.Weight));
            res *= maxWeight;
            res /= cl1.Count;
            return res;
        }

        /// <summary>
        ///  Returns the quanben distance between two cores
        ///  early quits if it's set to be bigger than the specified value
        ///  (in which case the returned the value would be a value no less than
        ///   the specified value but not necessarily the actual distance)
        ///  the cores are assumed to have same length
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <param name="quit">The threshold above which an early quit is triggered</param>
        /// <returns>The quanben distance no greater than quit</returns>
        public static double GetQuanbenDistance(this ICore c1, ICore c2,
            double quit = double.MaxValue)
        {
            var result = 0.0;
            var count = c1.CentersInput.Count;
            for (var i = 0; i < count && result < quit; i++)
            {
                var d = c1.CentersInput[i] - c2.CentersInput[i];
                var s = c1.CentersInput[i] + c2.CentersInput[i];
                var v = d / s;
                result += v;
            }
            return result;
        }

        #endregion
    }
}
