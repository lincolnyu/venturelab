using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GaussianCore.Classification
{
    public static class CoreDistanceHelper
    {
        #region Methods

        /// <summary>
        ///  Returns the absolute distance between the inputs of the two cores
        /// </summary>
        /// <param name="core1">The first core</param>
        /// <param name="core2">The second core</param>
        /// <returns>The distance</returns>
        public static double GetInputAbsDistance(this ICore core1, ICore core2)
        {
            var absd = 0.0;
            for (var i = 0; i < core1.CentersInput.Count; i++)
            {
                var c1 = core1.CentersInput[i];
                var c2 = core2.CentersInput[i];
                var d = Math.Abs(c2 - c1);
                absd += d;
            }
            return absd;
        }

        /// <summary>
        ///  Returns the absolute distance between the outputs of the two cores
        /// </summary>
        /// <param name="core1">The first core</param>
        /// <param name="core2">The second core</param>
        /// <returns>The distance</returns>
        public static double GetOutputAbsDistance(this ICore core1, ICore core2)
        {
            var absd = 0.0;
            for (var i = 0; i < core1.CentersOutput.Count; i++)
            {
                var c1 = core1.CentersOutput[i];
                var c2 = core2.CentersOutput[i];
                var d = Math.Abs(c2 - c1);
                absd += d;
            }
            return absd;
        }

        /// <summary>
        ///  Returns the mean absolute distance between the inputs of the two cores
        /// </summary>
        /// <param name="core1">The first core</param>
        /// <param name="core2">The second core</param>
        /// <returns>The distance</returns>
        public static double GetInputMeanAbsDistance(this ICore core1, ICore core2)
        {
            var d = core1.GetInputAbsDistance(core2);
            d /= core1.CentersInput.Count;
            return d;
        }

        /// <summary>
        ///  Returns the mean absolute distance between the outputs of the two cores
        /// </summary>
        /// <param name="core1"></param>
        /// <param name="core2"></param>
        /// <returns></returns>
        public static double GetOutputMeanAbsDistance(this ICore core1, ICore core2)
        {
            var d = core1.GetOutputAbsDistance(core2);
            d /= core1.CentersOutput.Count;
            return d;
        }

        /// <summary>
        ///  Returns the euclidian square distance of inputs between two cores 
        /// </summary>
        /// <param name="core1">The first core</param>
        /// <param name="core2">The second core</param>
        /// <returns>The square distance</returns>
        public static double GetInputSquareDistance(this ICore core1, ICore core2)
        {
            var sqd = 0.0;
            for (var i = 0; i < core1.CentersInput.Count; i++)
            {
                var c1 = core1.CentersInput[i];
                var c2 = core2.CentersInput[i];
                var d = c2 - c1;
                d *= d;
                sqd += d;
            }
            return sqd;
        }

        public static double GetInputDifferenceAbs(this ICore core1, ICore core2)
        {
            var sumd = 0.0;
            for (var i = 0; i < core1.CentersInput.Count; i++)
            {
                var c1 = core1.CentersInput[i];
                var c2 = core2.CentersInput[i];
                var d = c2 - c1;
                sumd += d;
            }
            return Math.Abs(sumd);
        }

        /// <summary>
        ///  returns the square of the sum of difference between each input component pair
        /// </summary>
        /// <param name="core1">The first core</param>
        /// <param name="core2">The second core</param>
        /// <returns>The square of the sum of the difference</returns>
        /// <remarks>
        ///  Note the inequality: 
        ///   a1^2+a2^2+...+aN^2 >= (a1+a2+...+aN)^2 / N
        /// </remarks>
        public static double GetInputDifferenceSquare(this ICore core1, ICore core2)
        {
            var sumd = 0.0;
            for (var i = 0; i < core1.CentersInput.Count; i++)
            {
                var c1 = core1.CentersInput[i];
                var c2 = core2.CentersInput[i];
                var d = c2 - c1;
                sumd += d;
            }
            return sumd*sumd;
        }

        /// <summary>
        ///  returns the square of the sum of difference between each output component pair
        /// </summary>
        /// <param name="core1">The first core</param>
        /// <param name="core2">The second core</param>
        /// <returns>The square of the sum of the difference</returns>
        public static double GetOutputSquareDistance(this ICore core1, ICore core2)
        {
            var sqd = 0.0;
            for (var i = 0; i < core1.CentersOutput.Count; i++)
            {
                var c1 = core1.CentersOutput[i];
                var c2 = core2.CentersOutput[i];
                var d = c2 - c1;
                d *= d;
                sqd += d;
            }
            return sqd;
        }


        public static double GetQuanbenDistance(this IList<ICore> cl1, IList<ICore> cl2)
        {
            if (cl2.Count < cl1.Count)
            {
                return GetQuanbenDistanceSmallFirst(cl2, cl1);
            }
            else
            {
                return GetQuanbenDistanceSmallFirst(cl1, cl2);
            }
        }

        /// <summary>
        ///  Returnst the distnace between <paramref name="cl1"/> and <paramref name="cl2"/>
        ///  with <paramref name="cl1"/> having no less cores than <paramref name="cl2"/>
        /// </summary>
        /// <param name="cl1">The first core collection</param>
        /// <param name="cl2">The second core colleciton</param>
        /// <returns>The distance</returns>
        private static double GetQuanbenDistanceSmallFirst(IList<ICore> cl1, IList<ICore> cl2)
        {
            var res = 0.0;

            var lockObj = new object();

            Parallel.ForEach(cl1, c1 =>
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
                        minD = d / w;
                    }
                }

                lock(lockObj)
                {
                    res += minD;
                }
            });

            // normalise about weight
            var maxWeight = Math.Max(cl1.Max(x => x.Weight), cl2.Max(x => x.Weight));
            res *= maxWeight;
            res /= cl1.Count * cl1[0].CentersInput.Count;
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
            for (var i = 0; i < c1.CentersInput.Count && result < quit; i++)
            {
                var d = c1.CentersInput[i] - c2.CentersInput[i];
                var s = c1.CentersInput[i] + c2.CentersInput[i];
                d *= d;
                s *= s;
                var v = d / s;
                result += v;
            }
            for(var i = 0; i < c1.CentersOutput.Count && result < quit; i++)
            {
                var d = c1.CentersOutput[i] - c2.CentersOutput[i];
                var s = c1.CentersOutput[i] + c2.CentersOutput[i];
                d *= d;
                s *= s;
                var v = d / s;
                result += v;
            }
            return result;
        }

        #endregion
    }
}
